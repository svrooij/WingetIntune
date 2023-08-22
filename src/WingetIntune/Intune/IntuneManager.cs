using Microsoft.Extensions.Logging;
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Text;
using System.Text.Json;
using WingetIntune.GraphExtensions;
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
    private readonly HttpClient httpClient;
    private readonly Mapper mapper = new Mapper();
    private readonly IAzureFileUploader azureFileUploader;
    private readonly Internal.MsStore.MicrosoftStoreClient microsoftStoreClient;
    private readonly PublicClientAuth publicClient;

    internal const string IntuneWinAppUtil = "IntuneWinAppUtil.exe";
    internal const string IntuneWinAppUtilUrl = "https://github.com/microsoft/Microsoft-Win32-Content-Prep-Tool/raw/master/IntuneWinAppUtil.exe";

    public IntuneManager(ILoggerFactory loggerFactory, IFileManager fileManager, IProcessManager processManager, HttpClient httpClient, IAzureFileUploader azureFileUploader, Internal.MsStore.MicrosoftStoreClient microsoftStoreClient, PublicClientAuth publicClient)
    {
        this.loggerFactory = loggerFactory;
        this.logger = loggerFactory.CreateLogger<IntuneManager>();
        this.fileManager = fileManager;
        this.processManager = processManager;
        this.httpClient = httpClient;
        this.azureFileUploader = azureFileUploader;
        this.microsoftStoreClient = microsoftStoreClient;
        this.publicClient = publicClient;
    }

    public async Task GenerateMsiPackage(string tempFolder, string outputFolder, Models.PackageInfo packageInfo, PackageOptions packageOptions, CancellationToken cancellationToken = default)
    {
        if (!packageInfo.InstallerType.IsMsi())
        {
            throw new ArgumentException("Package is not an MSI package", nameof(packageInfo));
        }
        if (packageInfo.Architecture == Architecture.Unknown)
        {
            ComputeInstallerDetails(ref packageInfo, packageOptions);
        }
        LogGeneratePackage(packageInfo.PackageIdentifier!, packageInfo.Version!, outputFolder);
        var packageTempFolder = fileManager.CreateFolderForPackage(tempFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);
        var packageFolder = fileManager.CreateFolderForPackage(outputFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);
        var contentPrepToolLocation = await DownloadContentPrepToolAsync(tempFolder, packageOptions.ContentPrepUri, cancellationToken);
        var installerPath = await DownloadInstallerAsync(packageTempFolder, packageInfo, cancellationToken);
        LoadMsiDetails(installerPath, ref packageInfo);
        await GenerateIntuneWinFile(contentPrepToolLocation, packageTempFolder, packageFolder, packageInfo.InstallerFilename!, cancellationToken);
        await DownloadLogoAsync(packageFolder, packageInfo.PackageIdentifier!, cancellationToken);
        await WriteReadmeAsync(packageFolder, packageInfo, cancellationToken);
        await WritePackageInfo(packageFolder, packageInfo, cancellationToken);
    }

    public async Task GenerateInstallerPackage(string tempFolder, string outputFolder, Models.PackageInfo packageInfo, PackageOptions? packageOptions = null, CancellationToken cancellationToken = default)
    {
        if (packageOptions is null)
        {
            packageOptions = PackageOptions.Create();
        }
        if (packageInfo.Source != PackageSource.Winget)
        {
            throw new ArgumentException("Package is not a winget package", nameof(packageInfo));
        }
        ComputeInstallerDetails(ref packageInfo, packageOptions);
        if (packageInfo.InstallerType.IsMsi())
        {
            await GenerateMsiPackage(tempFolder, outputFolder, packageInfo, packageOptions, cancellationToken);
            return;
        }
        LogGeneratePackage(packageInfo.PackageIdentifier!, packageInfo.Version!, outputFolder);
        await GenerateNoneMsiInstaller(tempFolder, outputFolder, packageInfo, packageOptions, cancellationToken);
    }

    private async Task GenerateNoneMsiInstaller(string tempFolder, string outputFolder, PackageInfo packageInfo, PackageOptions packageOptions, CancellationToken cancellationToken)
    {
        var packageTempFolder = fileManager.CreateFolderForPackage(tempFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);
        var packageFolder = fileManager.CreateFolderForPackage(outputFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);
        var contentPrepToolLocation = await DownloadContentPrepToolAsync(tempFolder, packageOptions.ContentPrepUri, cancellationToken);
        // TODO : If installer is not supported (yet) should it be downloaded?
        if (SupportedInstallers.Contains(packageInfo.InstallerType))
        {
            var installerPath = await DownloadInstallerAsync(packageTempFolder, packageInfo, cancellationToken);
        }
        else
        {
            // Generate scripts
            if (packageInfo.InstallCommandLine!.StartsWith("winget"))
            {
                // TODO Create Winget Install script
                await fileManager.WriteAllTextAsync(
                    Path.Combine(packageTempFolder, "install.ps1"),
                    GetPsCommandContent(packageInfo.InstallCommandLine, "installed", $"Package {packageInfo.PackageIdentifier} v{packageInfo.Version} installed successfully"),
                    cancellationToken);
                packageInfo.InstallCommandLine = $"powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File install.ps1";
                packageInfo.InstallerFilename = "install.ps1";
            }
        }

        if (packageInfo.UninstallCommandLine!.StartsWith("winget"))
        {
            await fileManager.WriteAllTextAsync(
                    Path.Combine(packageTempFolder, "uninstall.ps1"),
                    GetPsCommandContent(packageInfo.UninstallCommandLine, "uninstalled", $"Package {packageInfo.PackageIdentifier} uninstalled successfully"),
                    cancellationToken);
            packageInfo.UninstallCommandLine = $"powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File uninstall.ps1";
        }

        await GenerateIntuneWinFile(contentPrepToolLocation, packageTempFolder, packageFolder, packageInfo.InstallerFilename!, cancellationToken);
        await DownloadLogoAsync(packageFolder, packageInfo.PackageIdentifier!, cancellationToken);

        var detectionScript = IntuneManagerConstants.PsDetectionCommandTemplate.Replace("{packageId}", packageInfo.PackageIdentifier!).Replace("{version}", packageInfo.Version);
        await fileManager.WriteAllTextAsync(
            Path.Combine(packageFolder, "detection.ps1"),
            detectionScript,
            cancellationToken);
        packageInfo.DetectionScript = detectionScript;

        await WritePackageInfo(packageFolder, packageInfo, cancellationToken);
        await WriteReadmeAsync(packageFolder, packageInfo, cancellationToken);
    }

    private static string GetPsCommandContent(string command, string successSearch, string message)
    {
        var commandWithQuotes = string.Join(" ", command.Split(" ").Select(x => $"\"{x}\""));
        return IntuneManagerConstants.PsCommandTemplate.Replace("{command}", commandWithQuotes).Replace("{success}", successSearch).Replace("{message}", message);
    }

    public async Task<PackageInfo> LoadPackageInfoFromFolder(string packageFolder, string packageId, string version, CancellationToken cancellationToken = default)
    {
        var packageFile = Path.Combine(packageFolder, packageId, version, "app.json");
        if (!fileManager.FileExists(packageFile))
        {
            throw new FileNotFoundException("Package file not found", packageFile);
        }

        var data = await fileManager.ReadAllBytesAsync(packageFile, cancellationToken);
        return JsonSerializer.Deserialize<PackageInfo>(data, MyJsonContext.Default.PackageInfo)!;
    }

    public async Task<MobileApp> PublishAppAsync(string packagesFolder, PackageInfo packageInfo, IntunePublishOptions options, CancellationToken cancellationToken = default)
    {
        if (packageInfo.Source == PackageSource.Store)
        {
            return await PublishStoreAppAsync(options, packageId: packageInfo.PackageIdentifier, cancellationToken: cancellationToken);
        }

        GraphServiceClient graphServiceClient = CreateGraphClientFromOptions(options);

        Win32LobApp? app = mapper.ToWin32LobApp(packageInfo);
        var packageFolder = Path.Join(packagesFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);
        var logoFile = Path.Combine(packageFolder, "..", "logo.png");
        if (fileManager.FileExists(logoFile))
        {
            app.LargeIcon = new MimeContent
            {
                Type = "image/png",
                Value = await fileManager.ReadAllBytesAsync(logoFile, cancellationToken)
            };
        }

        var intuneFilePath = Path.Combine(packageFolder, Path.GetFileNameWithoutExtension(packageInfo.InstallerFilename!) + ".intunewin");
        if (!fileManager.FileExists(intuneFilePath))
        {
            throw new FileNotFoundException("IntuneWin file not found", intuneFilePath);
        }

        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        logger.LogDebug("Extracting intunewin file {file} to {tempFolder}", intuneFilePath, tempFolder);
        //System.IO.Compression.ZipFile.ExtractToDirectory(intuneFilePath, tempFolder);
        fileManager.ExtractFileToFolder(intuneFilePath, tempFolder);

        var info = IntuneMetadata.GetApplicationInfo(await fileManager.ReadAllBytesAsync(IntuneMetadata.GetMetadataPath(tempFolder), cancellationToken))!;

        var intuneFileData = await fileManager.ReadAllBytesAsync(IntuneMetadata.GetContentsPath(tempFolder), cancellationToken);

        try
        {
            app = await graphServiceClient.DeviceAppManagement.MobileApps.PostAsync(app, cancellationToken);
            logger.LogInformation("Created app {id}, starting with content", app!.Id);

            // TODO Check if delay is needed
            await Task.Delay(1000, cancellationToken);

            var contentVersion = await graphServiceClient.Intune_CreateWin32LobAppContentVersionAsync(app!.Id!, cancellationToken);

            logger.LogDebug("Created content version {id}", contentVersion!.Id);

            // TODO Check if delay is needed
            await Task.Delay(1000, cancellationToken);

            var mobileAppContentFile = await graphServiceClient.Intune_CreateWin32LobAppContentVersionFileAsync(app.Id!, contentVersion.Id!, new MobileAppContentFile
            {
                Name = info.FileName,
                IsDependency = false,
                Size = info.UnencryptedContentSize,
                SizeEncrypted = intuneFileData.LongLength,
                Manifest = null,
            }, cancellationToken);

            logger.LogDebug("Created content file {id}", mobileAppContentFile?.Id);
            // Wait for a bit (it's generating the azure storage uri)
            await Task.Delay(3000, cancellationToken);

            MobileAppContentFile? updatedMobileAppContentFile = await graphServiceClient.Intune_GetWin32LobAppContentVersionFileAsync(app.Id!,
                contentVersion!.Id!,
                mobileAppContentFile!.Id!,
                cancellationToken);

            logger.LogDebug("Loaded content file {id} {blobUri}", updatedMobileAppContentFile?.Id, updatedMobileAppContentFile?.AzureStorageUri);

            await azureFileUploader.UploadFileToAzureAsync(
                IntuneMetadata.GetContentsPath(tempFolder),
                new Uri(updatedMobileAppContentFile!.AzureStorageUri!),
                cancellationToken);

            logger.LogDebug("Uploaded content file {id} {blobUri}", updatedMobileAppContentFile.Id, updatedMobileAppContentFile.AzureStorageUri);

            await Task.Delay(5000, cancellationToken);

            // Commit the file
            await graphServiceClient.Intune_CommitWin32LobAppContentVersionFileAsync(app.Id!,
                contentVersion!.Id!,
                mobileAppContentFile!.Id!,
                mapper.ToFileEncryptionInfo(info.EncryptionInfo),
                cancellationToken);

            logger.LogDebug("Committed content file {id}", mobileAppContentFile.Id);

            MobileAppContentFile? commitedFile = await graphServiceClient.Intune_WaitForFinalCommitStateAsync(app.Id!, contentVersion!.Id!, mobileAppContentFile!.Id!, cancellationToken);

            logger.LogInformation("App file uploaded successfully");

            // Update the app with the new content version
            var uploadedApp = await graphServiceClient.DeviceAppManagement.MobileApps[app.Id].PatchAsync(new Win32LobApp
            {
                CommittedContentVersion = contentVersion.Id,
            }, cancellationToken);

            logger.LogInformation("App CommitedContentVersion patched successfully");

            // TODO: Add Categories by ID (lookup by name?)

            // Remove the folder with the extracted package.
            fileManager.DeleteFileOrFolder(tempFolder);
            return app!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing app");
            throw;
        }
    }

    public async Task<WinGetApp> PublishStoreAppAsync(IntunePublishOptions options, string? packageId = null, string? searchString = null, CancellationToken cancellationToken = default)
    {
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
            var id = await microsoftStoreClient.GetPackageIdForFirstMatchAsync(searchString, cancellationToken);
            return await PublishStoreAppAsync(options, id, null, cancellationToken);
        }

        var manifest = await microsoftStoreClient.GetManifestAsync(packageId!, cancellationToken);
        var app = mapper.ToWinGetApp(manifest!);
        var details = await microsoftStoreClient.GetStoreDetailsAsync(packageId!, cancellationToken);
        var imagePath = Path.GetTempFileName();
        await fileManager.DownloadFileAsync(details!.iconUrl, imagePath, overrideFile: true, cancellationToken: cancellationToken);
        app.LargeIcon = new MimeContent
        {
            Type = "image/png",
            Value = await fileManager.ReadAllBytesAsync(imagePath, cancellationToken)
        };

        GraphServiceClient graphServiceClient = CreateGraphClientFromOptions(options);

        try
        {
            var appCreated = await graphServiceClient.DeviceAppManagement.MobileApps.PostAsync(app, cancellationToken);
            return appCreated!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing app");
            throw;
        }
    }

    internal Task DownloadLogoAsync(string packageFolder, string packageId, CancellationToken cancellationToken)
    {
        var logoPath = Path.GetFullPath(Path.Combine(packageFolder, "..", "logo.png"));
        var logoUri = $"https://api.winstall.app/icons/{packageId}.png";//new Uri($"https://winget.azureedge.net/cache/icons/48x48/{packageId}.png");
        LogDownloadLogo(logoUri);
        return fileManager.DownloadFileAsync(logoUri, logoPath, throwOnFailure: false, overrideFile: false, cancellationToken);
    }

    internal async Task<string> DownloadContentPrepToolAsync(string tempFolder, Uri contentPrepUri, CancellationToken cancellationToken)
    {
        LogDownloadContentPrepTool(contentPrepUri);
        fileManager.CreateFolder(tempFolder);

        var contentPrepToolPath = Path.Combine(tempFolder, IntuneWinAppUtil);
        await fileManager.DownloadFileAsync(contentPrepUri.ToString(), contentPrepToolPath, throwOnFailure: true, overrideFile: false, cancellationToken);
        return contentPrepToolPath;
    }

    internal async Task<string> DownloadInstallerAsync(string tempPackageFolder, PackageInfo packageInfo, CancellationToken cancellationToken)
    {
        var installerPath = Path.Combine(tempPackageFolder, packageInfo.InstallerFilename!);
        LogDownloadInstaller(packageInfo.InstallerUrl!, installerPath);
        await fileManager.DownloadFileAsync(packageInfo.InstallerUrl!.ToString(), installerPath, throwOnFailure: true, overrideFile: false, cancellationToken);
        return installerPath;
    }

    public static (string?, string?) GetMsiInfo(string setupFile, ILogger logger)
    {
        try
        {
            using var msi = new WixSharp.UI.MsiParser(setupFile);
            return (msi.GetProductCode(), msi.GetProductVersion());
        }
        catch (DllNotFoundException)
        {
            // WixSharp.UI.MsiParser uses Microsoft.Deployment.WindowsInstaller.dll which is not available on Linux
            logger.LogWarning("Unable to get product code from {setupFile} because Microsoft.Deployment.WindowsInstaller.dll is not available on Linux", setupFile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting product code from {setupFile}", setupFile);
            throw;
        }
        return (null, null);
    }

    private void LoadMsiDetails(string installerPath, ref PackageInfo packageInfo)
    {
        var (productCode, msiVersion) = GetMsiInfo(installerPath, logger);
        packageInfo.MsiProductCode = productCode ?? packageInfo.MsiProductCode;
        packageInfo.MsiVersion = msiVersion ?? packageInfo.MsiVersion;
        packageInfo.InstallCommandLine = $"msiexec /i {packageInfo.InstallerFilename!} /qn /norestart";
        packageInfo.UninstallCommandLine = $"msiexec /x {productCode} /qn /norestart";
    }

    private void ComputeInstallerDetails(ref PackageInfo package, PackageOptions packageOptions)
    {
        var installer = package.GetBestFit(packageOptions.Architecture, packageOptions.InstallerContext)
            ?? package.GetBestFit(packageOptions.Architecture, InstallerContext.Unknown);
        if (installer == null && packageOptions.Architecture == Architecture.X64)
        {
            installer = package.GetBestFit(Architecture.X86, packageOptions.InstallerContext)
                ?? package.GetBestFit(Architecture.X86, InstallerContext.Unknown);
        }
        if (installer is null)
        {
            throw new ArgumentException($"No installer found for {package.PackageIdentifier} {package.Version} {packageOptions.Architecture}");
        }

        package.InstallerUrl = new Uri(installer.InstallerUrl!);
        package.InstallerFilename = package.InstallerUrl.Segments.Last();
        package.Hash = installer.InstallerSha256;
        package.Architecture = installer.InstallerArchitecture;
        package.InstallerContext = installer.InstallerContext == InstallerContext.Unknown ? (package.InstallerContext ?? packageOptions.InstallerContext) : installer.InstallerContext;
        package.InstallerType = installer.ParsedInstallerType;
        package.Installer = installer;
        if (package.InstallerType.IsMsi())
        {
            package.MsiVersion ??= installer.AppsAndFeaturesEntries?.FirstOrDefault()?.DisplayVersion;
            package.MsiProductCode ??= installer.ProductCode;
        }
        else
        {
            ComputeInstallerCommands(ref package, packageOptions);
        }
    }

    private static readonly InstallerType[] SupportedInstallers = new[] { InstallerType.Inno, InstallerType.Msi, InstallerType.Burn, InstallerType.Wix };

    private static readonly Dictionary<InstallerType, string> DefaultInstallerSwitches = new()
    {
        { InstallerType.Inno, "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-" },
        { InstallerType.Burn, "/quiet /norestart" },
        //{ InstallerType.Wix, "/quiet /norestart" },
    };

    private void ComputeInstallerCommands(ref PackageInfo package, PackageOptions packageOptions)
    {
        // TODO: Add support for other installer types and adjust `SupportedInstallers` accordingly

        string? installerSwitches = package.Installer?.InstallerSwitches?.GetPreferred();
        switch (package.InstallerType)
        {
            case InstallerType.Inno:
                if (installerSwitches?.Contains("/VERYSILENT") != true)
                {
                    installerSwitches += " " + DefaultInstallerSwitches[InstallerType.Inno];
                    installerSwitches = installerSwitches.Trim();
                }
                package.InstallCommandLine = $"\"{package.InstallerFilename}\" {installerSwitches ?? DefaultInstallerSwitches[InstallerType.Inno]}";
                // Don't know the uninstall command
                // Configure the uninstall command for Inno Setup
                //package.UninstallCommandLine = $"\"{package.InstallerFilename}\" /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /D={{0}}";
                break;

            case InstallerType.Burn:
                package.InstallCommandLine = $"\"{package.InstallerFilename}\" {installerSwitches ?? DefaultInstallerSwitches[InstallerType.Burn]}";
                // Have to check the uninstall command
                package.UninstallCommandLine = $"\"{package.InstallerFilename}\" /quiet /norestart /uninstall /passive"; // /burn.ignoredependencies=\"{package.PackageIdentifier}\"
                break;
        }

        if (string.IsNullOrWhiteSpace(package.InstallCommandLine))
        {
            // This seems like a hack I know, but it's the only way to get the install command for now.
            package.InstallCommandLine = $"winget install --id {package.PackageIdentifier} --version {package.Version} --source winget --exact --accept-package-agreements --accept-source-agreements --disable-interactivity --silent";
        }

        if (string.IsNullOrWhiteSpace(package.UninstallCommandLine))
        {
            // This seems like a hack I know, but it's the only way to get the uninstall command for now.
            package.UninstallCommandLine = $"winget uninstall --id {package.PackageIdentifier} --version {package.Version} --source winget --exact --accept-source-agreements --silent --force --disable-interactivity --scope {(package.InstallerContext == InstallerContext.User ? "user" : "system")}";
        }
    }

    private async Task WriteReadmeAsync(string packageFolder, PackageInfo packageInfo, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        if (packageInfo.InstallerType.IsMsi())
        {
            logger.LogInformation("Writing detection info for msi package {packageId} {productCode}", packageInfo.PackageIdentifier, packageInfo.MsiProductCode!);

            sb.AppendFormat("Package {0} {1} from {2}\r\n", packageInfo.PackageIdentifier, packageInfo.Version, packageInfo.Source);
            sb.AppendLine();
            sb.AppendFormat("MsiProductCode={0}\r\n", packageInfo.MsiProductCode);
            sb.AppendFormat("MsiVersion={0}\r\n", packageInfo.MsiVersion!);

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
        if (packageInfo.InstallerType.IsMsi())
        {
            sb.AppendFormat("msiexec /x {0} /quiet /qn\r\n", packageInfo.MsiProductCode);
        }
        else
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

    private async Task GenerateIntuneWinFile(string contentPrepToolLocation, string tempPackageFolder, string packageFolder, string installerFilename, CancellationToken cancellationToken)
    {
        LogGenerateIntuneWinFile(tempPackageFolder, packageFolder, installerFilename);
        var args = $"-c {tempPackageFolder} -s {installerFilename} -o {packageFolder} -q";
        var result = await processManager.RunProcessAsync(contentPrepToolLocation, args, cancellationToken);
        if (result.ExitCode != 0)
        {
            var exception = new Exception($"Generating .intunewin resulted in a non-zero exitcode.");
            exception.Data.Add("ExitCode", result.ExitCode);
            exception.Data.Add("Output", result.Output);
            exception.Data.Add("Error", result.Error);
            logger.LogWarning(exception, "Generating .intunewin resulted in a non-zero exitcode.");
            throw exception;
        }
    }

    private GraphServiceClient CreateGraphClientFromOptions(IntunePublishOptions options)
    {
        IAuthenticationProvider provider = publicClient;
        if (!string.IsNullOrEmpty(options.Username) || !string.IsNullOrEmpty(options.Tenant))
        {
            publicClient.SetAccountSuggestion(new AccountSuggestion(options.Username, options.Tenant));
        }
        if (options.Credential is not null)
        {
            provider = new TokenCredentialAuthenticationProvider(options.Credential, RequiredScopes, loggerFactory.CreateLogger<TokenCredentialAuthenticationProvider>());
        }
        else if (!string.IsNullOrEmpty(options.Token))
        {
            provider = new StaticAuthenticationProvider(options.Token);
        }
        return new GraphServiceClient(httpClient, provider, "https://graph.microsoft.com/beta");
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Generating IntuneWin package for {PackageId} {Version} in {OutputFolder}")]
    private partial void LogGeneratePackage(string PackageId, string Version, string OutputFolder);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Downloading content prep tool from {ContentPrepUri}")]
    private partial void LogDownloadContentPrepTool(Uri ContentPrepUri);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Downloading installer from {InstallerUri} to {Path}")]
    private partial void LogDownloadInstaller(Uri InstallerUri, string Path);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Generating IntuneWin package from {TempPackageFolder} to {OutputFolder} with {InstallerFilename}")]
    private partial void LogGenerateIntuneWinFile(string TempPackageFolder, string OutputFolder, string InstallerFilename);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Downloading logo from {LogoUri}")]
    private partial void LogDownloadLogo(string LogoUri);

    public static Uri DefaultIntuneWinAppUrl => new Uri(IntuneWinAppUtilUrl);
}
