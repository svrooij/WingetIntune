using Microsoft.Extensions.Logging;
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using System.Text;
using System.Text.Json;
using WingetIntune.Intune;
using WingetIntune.Models;
using WingetIntune.GraphExtensions;

namespace WingetIntune;

public partial class IntuneManager
{
    public static string[] RequiredScopes = new[] { "DeviceManagementConfiguration.ReadWrite.All", "DeviceManagementApps.ReadWrite.All" };
    private readonly ILogger<IntuneManager> logger;
    private readonly IFileManager fileManager;
    private readonly IProcessManager processManager;
    private readonly HttpClient httpClient;
    private readonly Mapper mapper = new Mapper();
    private readonly IAzureFileUploader azureFileUploader;

    private const string IntuneWinAppUtil = "IntuneWinAppUtil.exe";
    private const string IntuneWinAppUtilUrl = "https://github.com/microsoft/Microsoft-Win32-Content-Prep-Tool/raw/master/IntuneWinAppUtil.exe";

    public IntuneManager(ILogger<IntuneManager> logger, IFileManager fileManager, IProcessManager processManager, HttpClient httpClient, IAzureFileUploader azureFileUploader)
    {
        this.logger = logger;
        this.fileManager = fileManager;
        this.processManager = processManager;
        this.httpClient = httpClient;
        this.azureFileUploader = azureFileUploader;
    }

    public Task GenerateMsiPackage(string tempFolder, string outputFolder, Models.PackageInfo packageInfo, CancellationToken cancellationToken = default)
        => GenerateMsiPackage(tempFolder, outputFolder, packageInfo, DefaultIntuneWinAppUrl, cancellationToken);

    public async Task GenerateMsiPackage(string tempFolder, string outputFolder, Models.PackageInfo packageInfo, Uri contentPrepUri, CancellationToken cancellationToken = default)
    {
        if (packageInfo.InstallerType != InstallerType.Msi)
        {
            throw new ArgumentException("Package is not an MSI package", nameof(packageInfo));
        }
        LogGeneratePackage(packageInfo.PackageIdentifier!, packageInfo.Version!, outputFolder);
        var packageTempFolder = fileManager.CreateFolderForPackage(tempFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);
        var packageFolder = fileManager.CreateFolderForPackage(outputFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);
        var contentPrepToolLocation = await DownloadContentPrepTool(tempFolder, contentPrepUri, cancellationToken);
        var installerPath = await DownloadInstaller(packageTempFolder, packageInfo, cancellationToken);
        LoadMsiDetails(installerPath, ref packageInfo);
        await GenerateIntuneWinFile(contentPrepToolLocation, packageTempFolder, packageFolder, packageInfo.InstallerFilename!, cancellationToken);
        await DownloadLogoAsync(packageFolder, packageInfo.PackageIdentifier!, cancellationToken);
        await WriteMsiDetails(packageFolder, packageInfo, cancellationToken);
        await WritePackageInfo(packageFolder, packageInfo, cancellationToken);
    }

    public async Task GenerateInstallerPackage(string tempFolder, string outputFolder, Models.PackageInfo packageInfo, Uri contentPrepUri, CancellationToken cancellationToken = default)
    {
        if (packageInfo.Source != PackageSource.Winget)
        {
            throw new ArgumentException("Package is not a winget package", nameof(packageInfo));
        }
        if (packageInfo.InstallerType == InstallerType.Msi)
        {
            await GenerateMsiPackage(tempFolder, outputFolder, packageInfo, contentPrepUri, cancellationToken);
            return;
        }
        LogGeneratePackage(packageInfo.PackageIdentifier!, packageInfo.Version!, outputFolder);
        var packageTempFolder = fileManager.CreateFolderForPackage(tempFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);
        var packageFolder = fileManager.CreateFolderForPackage(outputFolder, packageInfo.PackageIdentifier!, packageInfo.Version!);
        var contentPrepToolLocation = await DownloadContentPrepTool(tempFolder, contentPrepUri, cancellationToken);
        var installerPath = await DownloadInstaller(packageTempFolder, packageInfo, cancellationToken);
        await GenerateIntuneWinFile(contentPrepToolLocation, packageTempFolder, packageFolder, packageInfo.InstallerFilename!, cancellationToken);
        await DownloadLogoAsync(packageFolder, packageInfo.PackageIdentifier!, cancellationToken);
        //await GenerateMsiDetails(packageFolder, packageInfo, installerPath, cancellationToken);
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
        var token = await options.GetToken(cancellationToken);
        GraphServiceClient graphServiceClient = new GraphServiceClient(httpClient, new Internal.Msal.StaticAuthenticationProvider(token), "https://graph.microsoft.com/beta");
        
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
            logger.LogDebug("Created app {id}", app!.Id);

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

            await Task.Delay(3000, cancellationToken);

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
            }, cancellationToken: cancellationToken);

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

    private Task DownloadLogoAsync(string packageFolder, string packageId, CancellationToken cancellationToken)
    {
        var logoPath = Path.Combine(packageFolder, "..", "logo.png");
        var logoUri = $"https://api.winstall.app/icons/{packageId}.png";//new Uri($"https://winget.azureedge.net/cache/icons/48x48/{packageId}.png");
        LogDownloadLogo(logoUri);
        return fileManager.DownloadFileAsync(logoPath, logoUri, throwOnFailure: false, overrideFile: false, cancellationToken);
    }

    private async Task<string> DownloadContentPrepTool(string tempFolder, Uri contentPrepUri, CancellationToken cancellationToken)
    {
        LogDownloadContentPrepTool(contentPrepUri);
        fileManager.CreateFolder(tempFolder);

        var contentPrepToolPath = Path.Combine(tempFolder, IntuneWinAppUtil);
        await fileManager.DownloadFileAsync(contentPrepToolPath, contentPrepUri.ToString(), throwOnFailure: true, overrideFile: false, cancellationToken);
        return contentPrepToolPath;
    }

    private async Task<string> DownloadInstaller(string tempPackageFolder, PackageInfo packageInfo, CancellationToken cancellationToken)
    {
        var installerPath = Path.Combine(tempPackageFolder, packageInfo.InstallerFilename!);
        LogDownloadInstaller(packageInfo.InstallerUrl!, installerPath);
        await fileManager.DownloadFileAsync(installerPath, packageInfo.InstallerUrl!.ToString(), throwOnFailure: true, overrideFile: false, cancellationToken);
        return installerPath;
    }

    public static (string, string) GetMsiInfo(string setupFile, ILogger logger)
    {
        try
        {
            using var msi = new WixSharp.UI.MsiParser(setupFile);
            return (msi.GetProductCode(), msi.GetProductVersion());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting product code from {setupFile}", setupFile);
            throw;
        }
    }

    private void LoadMsiDetails(string installerPath, ref PackageInfo packageInfo)
    {
        var (productCode, msiVersion) = GetMsiInfo(installerPath, logger);
        packageInfo.MsiProductCode = productCode;
        packageInfo.MsiVersion = msiVersion;
        packageInfo.InstallCommandLine = $"msiexec /i {packageInfo.InstallerFilename!} /qn /norestart";
        packageInfo.UninstallCommandLine = $"msiexec /x {productCode} /qn /norestart";
    }

    private async Task WriteMsiDetails(string packageFolder, PackageInfo packageInfo, CancellationToken cancellationToken)
    {
        logger.LogInformation("Writing detection info for msi package {packageId} {productCode}", packageInfo.PackageIdentifier, packageInfo.MsiProductCode!);
        var sb = new StringBuilder();
        sb.AppendFormat("Package {0} {1} from {2}\r\n", packageInfo.PackageIdentifier, packageInfo.Version, packageInfo.Source);
        sb.AppendLine();
        sb.AppendFormat("MsiProductCode={0}\r\n", packageInfo.MsiProductCode);
        sb.AppendFormat("MsiVersion={0}\r\n", packageInfo.MsiVersion!);

        var detectionFile = Path.Combine(packageFolder, "detection.txt");
        await fileManager.WriteAllTextAsync(detectionFile, sb.ToString(), cancellationToken);
        sb.Clear();

        logger.LogInformation("Writing package readme for msi package {packageId}", packageInfo.PackageIdentifier);
        sb.AppendFormat("Package {0} {1} from {2}\r\n", packageInfo.PackageIdentifier, packageInfo.Version, packageInfo.Source);
        sb.AppendLine();
        sb.AppendLine("Install script:");
        sb.AppendFormat("msiexec /i {0} /quiet /qn\r\n", packageInfo.InstallerUrl!.Segments.Last());
        sb.AppendLine();
        sb.AppendLine("Uninstall script:");
        sb.AppendFormat("msiexec /x {0} /quiet /qn\r\n", packageInfo.MsiProductCode);

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