using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using Microsoft.Graph.Beta.Models.ODataErrors;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Text;
using System.Text.Json;
using WingetIntune.Graph;
using WingetIntune.Implementations;
using WingetIntune.Interfaces;
using WingetIntune.Internal.Msal;
using WingetIntune.Intune;
using WingetIntune.Models;

namespace WingetIntune;

public partial class IntuneManager
{
    public static string[] RequiredScopes = new[] { "DeviceManagementConfiguration.ReadWrite.All", "DeviceManagementApps.ReadWrite.All" };
    private readonly ILogger<IntuneManager> logger;
    private readonly ILoggerFactory loggerFactory;
    private readonly IFileManager fileManager;
    private readonly IProcessManager processManager;
    private readonly IWingetRepository wingetRepository;
    private readonly HttpClient httpClient;
    private readonly Mapper mapper = new Mapper();
    private readonly IAzureFileUploader azureFileUploader;
    private readonly IIntunePackager intunePackager;
    private readonly PublicClientAuth publicClient;
    private readonly GraphAppUploader graphAppUploader;
    private readonly GraphStoreAppUploader graphStoreAppUploader;

    public IntuneManager(ILoggerFactory? loggerFactory, IFileManager fileManager, IProcessManager processManager, HttpClient httpClient, IAzureFileUploader azureFileUploader, PublicClientAuth publicClient, IIntunePackager intunePackager, IWingetRepository wingetRepository, GraphAppUploader graphAppUploader, GraphStoreAppUploader graphStoreAppUploader)
    {
        this.loggerFactory = loggerFactory ?? new NullLoggerFactory();
        this.logger = this.loggerFactory.CreateLogger<IntuneManager>();
        this.fileManager = fileManager;
        this.processManager = processManager;
        this.httpClient = httpClient;
        this.azureFileUploader = azureFileUploader;
        this.publicClient = publicClient;
        this.intunePackager = intunePackager;
        this.wingetRepository = wingetRepository;
        this.graphAppUploader = graphAppUploader;
        this.graphStoreAppUploader = graphStoreAppUploader;
    }

    public async Task<Models.WingetPackage> GenerateMsiPackage(string tempFolder, string outputFolder, Models.PackageInfo packageInfo, PackageOptions packageOptions, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(tempFolder);
        ArgumentException.ThrowIfNullOrEmpty(outputFolder);
        ArgumentNullException.ThrowIfNull(packageInfo);
        ArgumentNullException.ThrowIfNull(packageOptions);
#endif
        if (!packageInfo.InstallerType.IsMsi())
        {
            throw new ArgumentException("Package is not an MSI package", nameof(packageInfo));
        }
        if (packageInfo.Architecture == Architecture.Unknown)
        {
            ComputeInstallerDetails(ref packageInfo, packageOptions);
        }
        LogGeneratePackage(packageInfo.PackageIdentifier!, packageInfo.Version!, packageInfo.Architecture, packageInfo.InstallerContext, outputFolder);
        var packageTempFolder = fileManager.CreateFolderForPackage(tempFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);
        var packageFolder = fileManager.CreateFolderForPackage(outputFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);
        var installerPath = await DownloadInstallerAsync(packageTempFolder, packageInfo, cancellationToken);
        LoadMsiDetails(installerPath, ref packageInfo);
        var intunePackage = await intunePackager.CreatePackage(packageTempFolder, packageFolder, packageInfo.InstallerFilename!, packageInfo, cancellationToken);
        await DownloadLogoAsync(packageFolder, packageInfo.PackageIdentifier!, cancellationToken);
        await WriteReadmeAsync(packageFolder, packageInfo, cancellationToken);
        await WritePackageInfo(packageFolder, packageInfo, cancellationToken);

        return new Models.WingetPackage(packageInfo, packageFolder, intunePackage!) { InstallerArguments = packageInfo.InstallCommandLine?.Substring(packageInfo.InstallerFilename?.Length + 3 ?? 0), InstallerFile = packageInfo.InstallerFilename };
    }

    /// <summary>
    /// Generates an Intune package for a winget package
    /// </summary>
    /// <param name="tempFolder">Folder to temporary store files</param>
    /// <param name="outputFolder">Folder where the package should be</param>
    /// <param name="packageInfo">(Partial) information about the package</param>
    /// <param name="packageOptions">User-defined options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<Models.WingetPackage> GenerateInstallerPackage(string tempFolder, string outputFolder, Models.PackageInfo packageInfo, PackageOptions? packageOptions = null, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(tempFolder);
        ArgumentException.ThrowIfNullOrEmpty(outputFolder);
#endif
        if (packageOptions is null)
        {
            packageOptions = PackageOptions.Create();
        }

        if (packageInfo.Source != PackageSource.Winget)
        {
            throw new ArgumentException("Package is not a winget package", nameof(packageInfo));
        }

        if (!packageInfo.InstallersLoaded)
        {
            packageInfo = await wingetRepository.GetPackageInfoAsync(packageInfo.PackageIdentifier!, packageInfo.Version, "winget", cancellationToken);
        }

        ComputeInstallerDetails(ref packageInfo, packageOptions);

        if (packageInfo.InstallerType.IsMsi() && !packageOptions.PackageScript)
        {
            return await GenerateMsiPackage(tempFolder, outputFolder, packageInfo, packageOptions, cancellationToken);
        }
        LogGeneratePackage(packageInfo.PackageIdentifier!, packageInfo.Version!, packageInfo.Architecture, packageInfo.InstallerContext, outputFolder);
        return await GenerateNoneMsiInstaller(tempFolder, outputFolder, packageInfo, packageOptions, cancellationToken);
    }

    private async Task<WingetPackage> GenerateNoneMsiInstaller(string tempFolder, string outputFolder, PackageInfo packageInfo, PackageOptions packageOptions, CancellationToken cancellationToken)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(tempFolder);
        ArgumentException.ThrowIfNullOrEmpty(outputFolder);
        ArgumentNullException.ThrowIfNull(packageInfo);
#endif
        var packageTempFolder = fileManager.CreateFolderForPackage(tempFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);
        var packageFolder = fileManager.CreateFolderForPackage(outputFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);

        if (SupportedInstallers.Contains(packageInfo.InstallerType) && packageOptions.PackageScript != true)
        {
            _ = await DownloadInstallerAsync(packageTempFolder, packageInfo, cancellationToken);
        }
        else
        {
            // Generate scripts
            if (packageInfo.InstallCommandLine!.StartsWith("winget"))
            {
                // WinGet is not always available in the context of the user and you need to make sure to run to correct version.
                // This is way there is a helper script that always discovers the correct winget location to use.
                var installScript = GetPsCommandContent(packageInfo.InstallCommandLine, "installed", $"Package {packageInfo.PackageIdentifier} v{packageInfo.Version} installed successfully", packageId: packageInfo.PackageIdentifier, action: "install");
                await fileManager.WriteAllTextAsync(
                    Path.Combine(packageTempFolder, "install.ps1"),
                    installScript,
                    cancellationToken);
                packageInfo.InstallCommandLine = $"%windir%\\sysnative\\windowspowershell\\v1.0\\powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File install.ps1";
                packageInfo.InstallerFilename = "install.ps1";

                // Also output the script to the output folder
                await fileManager.WriteAllTextAsync(
                    Path.Combine(packageFolder, "install.ps1"),
                    installScript,
                    cancellationToken);
            }
        }

        if (packageInfo.UninstallCommandLine!.StartsWith("winget"))
        {
            // Helper script to discover the correct winget location
            var uninstallScript = GetPsCommandContent(packageInfo.UninstallCommandLine, "uninstalled", $"Package {packageInfo.PackageIdentifier} uninstalled successfully", packageId: packageInfo.PackageIdentifier, action: "uninstall");
            await fileManager.WriteAllTextAsync(
                    Path.Combine(packageTempFolder, "uninstall.ps1"),
                    uninstallScript,
                    cancellationToken);
            packageInfo.UninstallCommandLine = $"%windir%\\sysnative\\windowspowershell\\v1.0\\powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File uninstall.ps1";

            // Also output the script to the output folder
            await fileManager.WriteAllTextAsync(
                Path.Combine(packageFolder, "uninstall.ps1"),
                uninstallScript,
                cancellationToken);
        }

        var intuneFile = await intunePackager.CreatePackage(packageTempFolder, packageFolder, packageInfo.InstallerFilename!, packageInfo, cancellationToken);
        await DownloadLogoAsync(packageFolder, packageInfo.PackageIdentifier!, cancellationToken);

        var detectionScript = IntuneManagerConstants.PsDetectionCommandTemplate.Replace("{packageId}", packageInfo.PackageIdentifier!).Replace("{version}", packageInfo.Version);
        await fileManager.WriteAllTextAsync(
            Path.Combine(packageFolder, "detection.ps1"),
            detectionScript,
            cancellationToken);
        packageInfo.DetectionScript = detectionScript;

        await WritePackageInfo(packageFolder, packageInfo, cancellationToken);
        await WriteReadmeAsync(packageFolder, packageInfo, cancellationToken);

        return new WingetPackage(packageInfo, packageFolder, intuneFile) { InstallerFile = packageInfo.InstallerFilename, InstallerArguments = packageInfo.InstallCommandLine?.Substring(packageInfo.InstallerFilename?.Length + 3 ?? 0) };
    }

    private static string GetPsCommandContent(string command, string successSearch, string message, string? packageId = null, string? action = null)
    {
        var commandSplitted = command.Split(" ");
        string commandWithQuotes = "";
        if (commandSplitted.Length > 1 && commandSplitted[0].Equals("winget", StringComparison.OrdinalIgnoreCase))
        {
            commandWithQuotes = "$(Get-WingetCmd) ";
            commandSplitted = commandSplitted.Skip(1).ToArray();
        }
        commandWithQuotes += string.Join(" ", commandSplitted.Select(x => $"\"{x}\""));
        return IntuneManagerConstants.PsCommandTemplate
            .Replace("{command}", commandWithQuotes)
            .Replace("{success}", successSearch)
            .Replace("{message}", message)
            .Replace("{packageId}", packageId ?? Guid.NewGuid().ToString())
            .Replace("{action}", action ?? "unknown");
    }

    public Task<PackageInfo> LoadPackageInfoFromFolder(string packageFolder, string packageId, string version, CancellationToken cancellationToken = default)
    {
        var packageFile = Path.Combine(packageFolder, packageId, version, "app.json");
        return LoadPackageInfoFromFile(packageFile, cancellationToken);
    }

    public async Task<PackageInfo> LoadPackageInfoFromFile(string packageFile, CancellationToken cancellationToken = default)
    {
        if (!fileManager.FileExists(packageFile))
        {
            throw new FileNotFoundException("Package file not found", packageFile);
        }

        var data = await fileManager.ReadAllBytesAsync(packageFile, cancellationToken);
        return JsonSerializer.Deserialize<PackageInfo>(data, MyJsonContext.Default.PackageInfo)!;
    }

    public async Task<MobileApp> PublishAppAsync(string packagesFolder, PackageInfo packageInfo, IntunePublishOptions options, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(packagesFolder);
        ArgumentNullException.ThrowIfNull(packageInfo);
        ArgumentNullException.ThrowIfNull(options);
#endif
        if (packageInfo.Source == PackageSource.Store)
        {
            return await PublishStoreAppAsync(options, packageId: packageInfo.PackageIdentifier, cancellationToken: cancellationToken);
        }

        GraphServiceClient graphServiceClient = CreateGraphClientFromOptions(options);

        Win32LobApp? app = mapper.ToWin32LobApp(packageInfo);

        var packageFolder = Path.Join(packagesFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);
        var logoFile = Path.Combine(packageFolder, "..", "logo.png");
        var intuneFilePath = Path.Combine(packageFolder, Path.GetFileNameWithoutExtension(packageInfo.InstallerFilename!) + ".intunewin");

        app = await graphAppUploader.CreateNewAppAsync(graphServiceClient, app, intuneFilePath, logoFile, cancellationToken);

        if (options.Categories != null && options.Categories.Any())
        {
            await AddCategoriesToApp(graphServiceClient, app!.Id!, options.Categories, cancellationToken);
        }

        if (options.AvailableFor.Any() || options.RequiredFor.Any() || options.UninstallFor.Any())
        {
            await AssignAppAsync(graphServiceClient, app!.Id!, options.RequiredFor, options.AvailableFor, options.UninstallFor, options.AddAutoUpdateSetting, cancellationToken);
        }
        return app!;
    }

    public async Task<string> AddContentVersionToApp(IntunePublishOptions publishOptions, string appId, string intuneFilePath, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(publishOptions);
        ArgumentException.ThrowIfNullOrEmpty(appId);
        ArgumentException.ThrowIfNullOrEmpty(intuneFilePath);
#endif
        var graphServiceClient = CreateGraphClientFromOptions(publishOptions);

        return await AddContentVersionToApp(graphServiceClient, appId, intuneFilePath, cancellationToken);
    }

    internal async Task<string> AddContentVersionToApp(GraphServiceClient graphServiceClient, string appId, string intuneFilePath, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(graphServiceClient);
        ArgumentException.ThrowIfNullOrEmpty(appId);
        ArgumentException.ThrowIfNullOrEmpty(intuneFilePath);
#endif
        if (!fileManager.FileExists(intuneFilePath))
        {
            throw new FileNotFoundException("IntuneWin file not found", intuneFilePath);
        }

        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        logger.LogDebug("Extracting intunewin file {file} to {tempFolder}", intuneFilePath, tempFolder);
        fileManager.ExtractFileToFolder(intuneFilePath, tempFolder);

        var info = IntuneMetadata.GetApplicationInfo(await fileManager.ReadAllBytesAsync(IntuneMetadata.GetMetadataPath(tempFolder), cancellationToken))!;
        var intuneFileData = await fileManager.ReadAllBytesAsync(IntuneMetadata.GetContentsPath(tempFolder), cancellationToken);

        var contentVersion = await graphServiceClient.Intune_CreateWin32LobAppContentVersionAsync(appId, cancellationToken);

        logger.LogDebug("Created content version {id}", contentVersion!.Id);

        var mobileAppContentFileRequest = new MobileAppContentFile
        {
            Name = info.FileName,
            IsDependency = false,
            Size = info.UnencryptedContentSize,
            SizeEncrypted = intuneFileData.LongLength,
            Manifest = null,
        };

        logger.LogDebug("Creating content file {name} {size} {sizeEncrypted}", mobileAppContentFileRequest.Name, mobileAppContentFileRequest.Size, mobileAppContentFileRequest.SizeEncrypted);

        var mobileAppContentFile = await graphServiceClient.Intune_CreateWin32LobAppContentVersionFileAsync(appId, contentVersion.Id!, mobileAppContentFileRequest, cancellationToken);

        logger.LogDebug("Created content file {id}", mobileAppContentFile?.Id);
        // Wait for a bit (it's generating the azure storage uri)
        await Task.Delay(3000, cancellationToken);

        MobileAppContentFile? updatedMobileAppContentFile = await graphServiceClient.Intune_GetWin32LobAppContentVersionFileAsync(appId,
            contentVersion!.Id!,
            mobileAppContentFile!.Id!,
            cancellationToken);

        logger.LogDebug("Loaded content file {id} {blobUri}", updatedMobileAppContentFile?.Id, updatedMobileAppContentFile?.AzureStorageUri);

        await azureFileUploader.UploadFileToAzureAsync(
            IntuneMetadata.GetContentsPath(tempFolder),
            new Uri(updatedMobileAppContentFile!.AzureStorageUri!),
            cancellationToken);

        logger.LogDebug("Uploaded content file {id} {blobUri}", updatedMobileAppContentFile.Id, updatedMobileAppContentFile.AzureStorageUri);
        fileManager.DeleteFileOrFolder(tempFolder);

        await Task.Delay(5000, cancellationToken);

        var encryptionInfo = mapper.ToFileEncryptionInfo(info.EncryptionInfo);

        logger.LogDebug("Mapped encryption info {encryptionInfo}", JsonSerializer.Serialize(encryptionInfo));

        // Commit the file
        await graphServiceClient.Intune_CommitWin32LobAppContentVersionFileAsync(appId,
            contentVersion!.Id!,
            mobileAppContentFile!.Id!,
            encryptionInfo,
            cancellationToken);

        logger.LogDebug("Committed content file {id}", mobileAppContentFile.Id);

        MobileAppContentFile? commitedFile = await graphServiceClient.Intune_WaitForFinalCommitStateAsync(appId, contentVersion!.Id!, mobileAppContentFile!.Id!, cancellationToken);

        logger.LogInformation("Added content version {contentVersionId} to app {appId}", contentVersion.Id, appId);
        return contentVersion.Id!;
    }

    public async Task<WinGetApp> PublishStoreAppAsync(IntunePublishOptions options, string? packageId = null, string? searchString = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrEmpty(packageId) && string.IsNullOrEmpty(searchString))
        {
            throw new ArgumentException("Either id or searchString must be specified");
        }

        if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(packageId))
        {
            throw new ArgumentException("Only one of id or searchString must be specified");
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            var id = await graphStoreAppUploader.GetStoreIdForNameAsync(searchString, cancellationToken);
            return await PublishStoreAppAsync(options, id, null, cancellationToken);
        }

        GraphServiceClient graphServiceClient = CreateGraphClientFromOptions(options);

        try
        {
            var appCreated = await graphStoreAppUploader.CreateStoreAppAsync(graphServiceClient, packageId!, cancellationToken);
            if (appCreated == null)
            {
                throw new Exception("App was not created");
            }
            if (options.Categories != null && options.Categories.Any())
            {
                await graphServiceClient.AddIntuneCategoriesToAppAsync(appCreated!.Id!, options.Categories, cancellationToken);
            }
            if (options.AvailableFor.Any() || options.RequiredFor.Any() || options.UninstallFor.Any())
            {
                await graphServiceClient.AssignAppAsync(appCreated!.Id!, options.RequiredFor, options.AvailableFor, options.UninstallFor, options.AddAutoUpdateSetting, cancellationToken);
            }
            return appCreated;
        }
        catch (ODataError ex)
        {
            logger.LogError(ex, "Error publishing app {message}", ex.Error?.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing app");
            throw;
        }
    }

    public Task<IEnumerable<IntuneApp>> GetPublishedAppsAsync(IntunePublishOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        GraphServiceClient graphServiceClient = CreateGraphClientFromOptions(options);

        return graphServiceClient.DeviceAppManagement.MobileApps.GetWinTunerAppsAsync(cancellationToken);
    }

    private async Task AddCategoriesToApp(GraphServiceClient graphServiceClient, string appId, string[] categories, CancellationToken cancellationToken)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(graphServiceClient);
        ArgumentException.ThrowIfNullOrEmpty(appId);
        ArgumentNullException.ThrowIfNull(categories);
#endif
        logger.LogInformation("Adding categories {categories} to app {appId}", string.Join(",", categories), appId);

        try
        {
            await GraphWorkflows.AddIntuneCategoriesToAppAsync(graphServiceClient, appId, categories, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding categories to app");
            // Don't throw, just continue.
            //throw;
        }
    }

    private async Task AssignAppAsync(GraphServiceClient graphServiceClient, string appId, string[]? requiredFor, string[]? availableFor, string[]? uninstallFor, bool addAutoUpdateSetting, CancellationToken cancellationToken)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(graphServiceClient);
        ArgumentException.ThrowIfNullOrEmpty(appId);
        ArgumentNullException.ThrowIfNull(cancellationToken);
#endif
        try
        {
            var assignments = await GraphWorkflows.AssignAppAsync(graphServiceClient, appId, requiredFor, availableFor, uninstallFor, addAutoUpdateSetting, cancellationToken);
            logger.LogInformation("Assigned app {appId} to {assignmentCount} assignments", appId, assignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning app to groups");
            // Don't throw, just continue.
            //throw;
        }
    }

    internal Task DownloadLogoAsync(string packageFolder, string packageId, CancellationToken cancellationToken)
    {
        var logoPath = Path.GetFullPath(Path.Combine(packageFolder, "..", "logo.png"));
        var logoUri = $"https://api.winstall.app/icons/{packageId}.png";//new Uri($"https://winget.azureedge.net/cache/icons/48x48/{packageId}.png");
        LogDownloadLogo(logoUri);
        return fileManager.DownloadFileAsync(logoUri, logoPath, throwOnFailure: false, overrideFile: false, cancellationToken: cancellationToken);
    }

    internal async Task<string> DownloadInstallerAsync(string tempPackageFolder, PackageInfo packageInfo, CancellationToken cancellationToken)
    {
        var installerPath = Path.Combine(tempPackageFolder, packageInfo.InstallerFilename!);
        LogDownloadInstaller(packageInfo.InstallerUrl!, installerPath);
        await fileManager.DownloadFileAsync(packageInfo.InstallerUrl!.ToString(), installerPath, expectedHash: packageInfo.Installer!.InstallerSha256!, throwOnFailure: true, overrideFile: false, cancellationToken: cancellationToken);
        return installerPath;
    }

    public static (string?, string?) GetMsiInfo(string setupFile, ILogger? logger)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(setupFile);
#endif
        try
        {
            using var msi = new WixSharp.UI.MsiParser(setupFile);
            return (msi.GetProductCode(), msi.GetProductVersion());
        }
        catch (DllNotFoundException)
        {
            // WixSharp.UI.MsiParser uses Microsoft.Deployment.WindowsInstaller.dll which is not available on Linux
            logger?.LogWarning("Unable to get product code from {setupFile} because Microsoft.Deployment.WindowsInstaller.dll is not available on Linux", setupFile);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error getting product code from {setupFile}", setupFile);
            throw;
        }
        return (null, null);
    }

    private void LoadMsiDetails(string installerPath, ref PackageInfo packageInfo)
    {
        if (string.IsNullOrEmpty(packageInfo.MsiProductCode) || string.IsNullOrEmpty(packageInfo.MsiVersion))
        {
            var (productCode, msiVersion) = GetMsiInfo(installerPath, logger);
            packageInfo.MsiProductCode = productCode ?? packageInfo.MsiProductCode;
            packageInfo.MsiVersion = msiVersion ?? packageInfo.MsiVersion;
        }
        packageInfo.InstallCommandLine = $"msiexec /i {packageInfo.InstallerFilename!} /qn /norestart";
        packageInfo.UninstallCommandLine = $"msiexec /x {packageInfo.MsiProductCode!} /qn /norestart";
    }

    private void ComputeInstallerDetails(ref PackageInfo package, PackageOptions packageOptions)
    {
        var installer = package.GetBestInstaller(packageOptions);
        if (installer is null)
        {
            throw new ArgumentException($"No installer found for {package.PackageIdentifier} {package.Version} {packageOptions.Architecture}");
        }

        package.InstallerUrl = new Uri(installer.InstallerUrl!);
        package.InstallerFilename = Path.GetFileName(package.InstallerUrl.LocalPath.Replace(" ", ""));

        if (string.IsNullOrEmpty(package.InstallerFilename))
        {
            package.InstallerFilename = $"{package.PackageIdentifier}_{package.Version}.{GuessInstallerExtension(installer.ParseInstallerType())}";
        }

        // Maybe this should be done for other installers as well?
        if ((installer.InstallerType!.Equals("exe", StringComparison.OrdinalIgnoreCase) || installer.InstallerType!.Equals("burn", StringComparison.OrdinalIgnoreCase)) && package.InstallerFilename!.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == false)
        {
            package.InstallerFilename += ".exe";
        }
        package.Hash = installer.InstallerSha256;
        package.Architecture = installer.InstallerArchitecture();
        package.InstallerContext = installer.ParseInstallerContext() == InstallerContext.Unknown ? (package.InstallerContext ?? packageOptions.InstallerContext) : installer.ParseInstallerContext();
        package.InstallerType = installer.ParseInstallerType();
        package.Installer = installer;
        if (!package.InstallerType.IsMsi() || packageOptions.PackageScript)
        {
            ComputeInstallerCommands(ref package, packageOptions);
        }

        package.MsiVersion ??= installer.AppsAndFeaturesEntries?.FirstOrDefault()?.DisplayVersion;
        package.MsiProductCode ??= installer.ProductCode ?? installer.AppsAndFeaturesEntries?.FirstOrDefault()?.ProductCode;

    }

    private static readonly InstallerType[] SupportedInstallers = new[] { InstallerType.Inno, InstallerType.Msi, InstallerType.Burn, InstallerType.Wix, InstallerType.Nullsoft, InstallerType.Exe };

    private static string GuessInstallerExtension(InstallerType installerType) => installerType switch
    {
        InstallerType.Inno => "exe",
        InstallerType.Msi => "msi",
        InstallerType.Msix => "msix",
        InstallerType.Appx => "appx",
        InstallerType.Burn => "exe",
        InstallerType.Wix => "msi",
        InstallerType.Nullsoft => "exe",
        InstallerType.Exe => "exe",
        InstallerType.Zip => "zip",
        _ => throw new ArgumentException("Unknown installer type", nameof(installerType))
    };
    private static readonly Dictionary<InstallerType, string> DefaultInstallerSwitches = new()
    {
        { InstallerType.Inno, "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-" },
        { InstallerType.Burn, "/quiet /norestart /install" },
        { InstallerType.Nullsoft, "/S" },
    };

    /// <summary>
    /// Compute the installer commands for the package
    /// </summary>
    /// <param name="package">Package info</param>
    /// <param name="packageOptions">User-defined options</param>
    private void ComputeInstallerCommands(ref PackageInfo package, PackageOptions packageOptions)
    {
        // If package script is enabled, we will just use the winget install & uninstall commands
        // This way your packages in Intune will not contain the installer files
        // And it also helps with installers that otherwise would just not install silently or install at all
        if (packageOptions.PackageScript != true)
        {
            string? installerSwitches = packageOptions.OverrideArguments ?? package.Installer?.InstallerSwitches?.GetPreferred();
            switch (package.InstallerType)
            {
                case InstallerType.Inno:
                    if (installerSwitches?.Contains("/VERYSILENT") != true)
                    {
                        installerSwitches += " " + DefaultInstallerSwitches[InstallerType.Inno];
                        installerSwitches = installerSwitches.Trim();
                    }
                    package.InstallCommandLine = $"\"{package.InstallerFilename}\" {installerSwitches}";
                    // Don't know the uninstall command
                    // Configure the uninstall command for Inno Setup
                    //package.UninstallCommandLine = $"\"{package.InstallerFilename}\" /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /D={{0}}";
                    break;

                case InstallerType.Burn:
                    if (installerSwitches?.Contains("/quiet") != true)
                    {
                        installerSwitches += " " + DefaultInstallerSwitches[InstallerType.Burn];
                        installerSwitches = string.Join(" ", installerSwitches.Split(' ').Distinct()).Trim();
                    }
                    package.InstallCommandLine = $"\"{package.InstallerFilename}\" {installerSwitches}";
                    // Have to check the uninstall command
                    package.UninstallCommandLine = $"\"{package.InstallerFilename}\" /quiet /norestart /uninstall /passive"; // /burn.ignoredependencies=\"{package.PackageIdentifier}\"
                    break;

                case InstallerType.Nullsoft:
                    package.InstallCommandLine = $"\"{package.InstallerFilename}\" {installerSwitches ?? DefaultInstallerSwitches[InstallerType.Nullsoft]}";
                    break;

                case InstallerType.Exe:
                    package.InstallCommandLine = $"\"{package.InstallerFilename}\" {installerSwitches}";
                    // Have to check the uninstall command
                    //package.UninstallCommandLine = $"\"{package.InstallerFilename}\" /quiet /norestart /uninstall /passive"; // /burn.ignoredependencies=\"{package.PackageIdentifier}\"
                    break;
            }
        }

        // If the installer type is unsupported or the package script is enabled, we will generate a script to install the package
        if (string.IsNullOrWhiteSpace(package.InstallCommandLine))
        {
            var installArguments = WingetHelper.GetInstallArgumentsForPackage(package.PackageIdentifier!, package.Version, installerContext: package.InstallerContext ?? InstallerContext.Unknown);
            // This seems like a hack I know, but it's the only way to get the install command for now.
            package.InstallCommandLine = $"winget {installArguments}";
        }

        // Uninstall command is almost always empty, so we just use winget to uninstall the package
        if (string.IsNullOrWhiteSpace(package.UninstallCommandLine))
        {
            var uninstallArguments = WingetHelper.GetUninstallArgumentsForPackage(package.PackageIdentifier!, installerContext: package.InstallerContext ?? InstallerContext.Unknown);
            package.UninstallCommandLine = $"winget {uninstallArguments}";
        }
    }

    private async Task WriteReadmeAsync(string packageFolder, PackageInfo packageInfo, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        if (packageInfo.InstallerType.IsMsi() || !string.IsNullOrEmpty(packageInfo.MsiProductCode))
        {
            logger.LogInformation("Writing detection info with msi details {packageId} {productCode}", packageInfo.PackageIdentifier, packageInfo.MsiProductCode!);

            sb.AppendFormat("Package {0} {1} from {2}\r\n", packageInfo.PackageIdentifier, packageInfo.Version, packageInfo.Source);
            sb.AppendLine();
            sb.AppendFormat("MsiProductCode={0}\r\n", packageInfo.MsiProductCode);
            sb.AppendFormat("MsiVersion={0}\r\n", packageInfo.MsiVersion);

            var detectionFile = Path.Combine(packageFolder, "detection.txt");
            await fileManager.WriteAllTextAsync(detectionFile, sb.ToString(), cancellationToken);
            sb.Clear();
        }

        logger.LogInformation("Writing package readme for package {packageId}", packageInfo.PackageIdentifier);
        sb.AppendFormat("Package {0} {1} from {2}\r\n", packageInfo.PackageIdentifier, packageInfo.Version, packageInfo.Source);
        sb.AppendLine();
        sb.AppendFormat("Display name: {0}\r\n", packageInfo.DisplayName);
        sb.AppendFormat("Publisher: {0}\r\n", packageInfo.Publisher);
        sb.AppendFormat("Homepage: {0}\r\n", packageInfo.InformationUrl);
        sb.AppendLine();
        sb.AppendLine("Install script:");
        if (packageInfo.InstallerType.IsMsi())
        {
            sb.AppendFormat("msiexec /i {0} /quiet /qn\r\n", packageInfo.InstallerFilename);
        }
        else
        {
            sb.AppendFormat("{0}\r\n", packageInfo.InstallCommandLine);
        }
        sb.AppendLine();
        sb.AppendLine("Uninstall script:");
        if (packageInfo.InstallerType.IsMsi() || !string.IsNullOrEmpty(packageInfo.MsiProductCode))
        {
            sb.AppendFormat("msiexec /x {0} /quiet /qn\r\nor\r\n", packageInfo.MsiProductCode);
        }

        if (!string.IsNullOrEmpty(packageInfo.UninstallCommandLine))
        {
            sb.AppendFormat("{0}\r\n", packageInfo.UninstallCommandLine);
        }

        sb.AppendLine();
        sb.AppendLine("Description:");
        sb.AppendLine(packageInfo.Description);

        var readme = Path.Combine(packageFolder, "readme.txt");
        await fileManager.WriteAllTextAsync(readme, sb.ToString(), cancellationToken);
    }

    private async Task WritePackageInfo(string packageFolder, PackageInfo packageInfo, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(packageInfo, MyJsonContext.Default.PackageInfo);
        var jsonFile = Path.Combine(packageFolder, "app.json");
        await fileManager.WriteAllBytesAsync(jsonFile, json, cancellationToken);
    }

    private GraphServiceClient CreateGraphClientFromOptions(IntunePublishOptions options)
    {
        IAuthenticationProvider provider = publicClient;
        if (!string.IsNullOrEmpty(options.Username) || !string.IsNullOrEmpty(options.Tenant))
        {
            publicClient.SetAccountSuggestion(new AccountSuggestion(options.Tenant, options.Username));
        }
        if (options.Credential is not null)
        {
            provider = new Microsoft.Graph.Authentication.AzureIdentityAuthenticationProvider(options.Credential, null, null, RequiredScopes);
        }
        else if (!string.IsNullOrEmpty(options.Token))
        {
            provider = new StaticAuthenticationProvider(options.Token);
        }
        return new GraphServiceClient(httpClient, provider, "https://graph.microsoft.com/beta");
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Generating IntuneWin package for {PackageId} {Version} {Architecture} {Context} in {OutputFolder}")]
    private partial void LogGeneratePackage(string PackageId, string Version, Architecture? Architecture, InstallerContext? Context, string OutputFolder);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Downloading content prep tool from {ContentPrepUri}")]
    private partial void LogDownloadContentPrepTool(Uri ContentPrepUri);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Downloading installer from {InstallerUri} to {Path}")]
    private partial void LogDownloadInstaller(Uri InstallerUri, string Path);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Generating IntuneWin package from {TempPackageFolder} to {OutputFolder} with {InstallerFilename}")]
    private partial void LogGenerateIntuneWinFile(string TempPackageFolder, string OutputFolder, string InstallerFilename);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Downloading logo from {LogoUri}")]
    private partial void LogDownloadLogo(string LogoUri);
}
