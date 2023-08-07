using Microsoft.Extensions.Logging;
using System.Text;
using WingetIntune.Models;

namespace WingetIntune;

public partial class IntuneManager
{
    private readonly ILogger<IntuneManager> logger;
    private readonly IFileManager fileManager;
    private readonly IProcessManager processManager;
    private readonly HttpClient httpClient;

    private const string IntuneWinAppUtil = "IntuneWinAppUtil.exe";
    private const string IntuneWinAppUtilUrl = "https://github.com/microsoft/Microsoft-Win32-Content-Prep-Tool/raw/master/IntuneWinAppUtil.exe";

    public IntuneManager(ILogger<IntuneManager> logger, IFileManager fileManager, IProcessManager processManager, HttpClient httpClient)
    {
        this.logger = logger;
        this.fileManager = fileManager;
        this.processManager = processManager;
        this.httpClient = httpClient;
    }

    public Task GenerateMsiPackage(string tempFolder, string outputFolder, Models.PackageInfo packageInfo, CancellationToken cancellationToken = default)
        => GenerateMsiPackage(tempFolder, outputFolder, packageInfo, DefaultIntuneWinAppUrl, cancellationToken);

    public async Task GenerateMsiPackage(string tempFolder, string outputFolder, Models.PackageInfo packageInfo, Uri contentPrepUri, CancellationToken cancellationToken = default)
    {
        if (packageInfo.InstallerType != InstallerType.Msi)
        {
            throw new ArgumentException("Package is not an MSI package", nameof(packageInfo));
        }
        LogGenerateMsiPackage(packageInfo.PackageId!, packageInfo.Version!, outputFolder);
        var packageTempFolder = fileManager.CreateFolderForPackage(tempFolder, packageInfo.PackageId!, packageInfo.Version!);
        var packageFolder = fileManager.CreateFolderForPackage(outputFolder, packageInfo.PackageId!, packageInfo.Version!);
        var contentPrepToolLocation = await DownloadContentPrepTool(tempFolder, contentPrepUri, cancellationToken);
        var installerPath = await DownloadInstaller(packageTempFolder, packageInfo, cancellationToken);
        await GenerateIntuneWinFile(contentPrepToolLocation, packageTempFolder, packageFolder, packageInfo.InstallerUrl!.Segments.Last(), cancellationToken);
        await DownloadLogoAsync(packageFolder, packageInfo.PackageId!, cancellationToken);
        await GenerateMsiDetails(packageFolder, packageInfo, installerPath, cancellationToken);
    }

    private Task DownloadLogoAsync(string packageFolder, string packageId, CancellationToken cancellationToken)
    {
        var logoPath = Path.Combine(packageFolder, "..", "logo.png");
        var logoUri = new Uri($"https://api.winstall.app/icons/{packageId}.png");//new Uri($"https://winget.azureedge.net/cache/icons/48x48/{packageId}.png");
        LogDownloadLogo(logoUri);
        return DownloadFileIfNotExists(logoPath, logoUri, false, cancellationToken);
    }

    private async Task<string> DownloadContentPrepTool(string tempFolder, Uri contentPrepUri, CancellationToken cancellationToken)
    {
        LogDownloadContentPrepTool(contentPrepUri);
        fileManager.CreateFolder(tempFolder);

        var contentPrepToolPath = Path.Combine(tempFolder, IntuneWinAppUtil);
        await DownloadFileIfNotExists(contentPrepToolPath, contentPrepUri, true, cancellationToken);
        return contentPrepToolPath;
    }

    private async Task<string> DownloadInstaller(string tempPackageFolder, PackageInfo packageInfo, CancellationToken cancellationToken)
    {
        var installerPath = Path.Combine(tempPackageFolder, packageInfo.InstallerUrl!.Segments.Last());
        LogDownloadInstaller(packageInfo.InstallerUrl!, installerPath);
        await DownloadFileIfNotExists(installerPath, packageInfo.InstallerUrl!, true, cancellationToken);
        return installerPath;
    }

    private (string, string) GetMsiInfo(string setupFile)
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

    private async Task GenerateMsiDetails(string packageFolder, PackageInfo packageInfo, string installerPath, CancellationToken cancellationToken)
    {
        var (productCode, msiVersion) = GetMsiInfo(installerPath);
        logger.LogInformation("Writing detection info for msi package {packageId} {productCode}", packageInfo.PackageId, productCode);
        var sb = new StringBuilder();
        sb.AppendFormat("Package {0} {1} from {2}\r\n", packageInfo.PackageId, packageInfo.Version, packageInfo.Source);
        sb.AppendLine();
        sb.AppendFormat("MsiProductCode={0}\r\n", productCode);
        sb.AppendFormat("MsiVersion={0}\r\n", msiVersion);

        var detectionFile = Path.Combine(packageFolder, "detection.txt");
        await fileManager.WriteAllTextAsync(detectionFile, sb.ToString(), cancellationToken);
        sb.Clear();

        logger.LogInformation("Writing package readme for msi package {packageId}", packageInfo.PackageId);
        sb.AppendFormat("Package {0} {1} from {2}\r\n", packageInfo.PackageId, packageInfo.Version, packageInfo.Source);
        sb.AppendLine();
        sb.AppendLine("Install script:");
        sb.AppendFormat("msiexec /i {0} /quiet /qn\r\n", packageInfo.InstallerUrl!.Segments.Last());
        sb.AppendLine();
        sb.AppendLine("Uninstall script:");
        sb.AppendFormat("msiexec /x {0} /quiet /qn\r\n", productCode);

        var readme = Path.Combine(packageFolder, "readme.txt");
        await fileManager.WriteAllTextAsync(readme, sb.ToString(), cancellationToken);
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

    private async Task DownloadFileIfNotExists(string path, Uri uri, bool throwOnFailure, CancellationToken cancellationToken)
    {
        LogDownloadStarted(uri, path);
        if (fileManager.FileExists(path))
        {
            return;
        }

        var response = await httpClient.GetAsync(uri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Error downloading {uri} to {path}", uri, path);
            if (throwOnFailure)
                response.EnsureSuccessStatusCode();
            return;
        }
        var imageData = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        await fileManager.WriteAllBytesAsync(path, imageData, cancellationToken);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Generating IntuneWin package for {PackageId} {Version} in {OutputFolder}")]
    private partial void LogGenerateMsiPackage(string PackageId, string Version, string OutputFolder);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Downloading started for {Uri} to {Path}")]
    private partial void LogDownloadStarted(Uri Uri, string Path);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Downloading content prep tool from {ContentPrepUri}")]
    private partial void LogDownloadContentPrepTool(Uri ContentPrepUri);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Downloading installer from {InstallerUri} to {Path}")]
    private partial void LogDownloadInstaller(Uri InstallerUri, string Path);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Generating IntuneWin package from {TempPackageFolder} to {OutputFolder} with {InstallerFilename}")]
    private partial void LogGenerateIntuneWinFile(string TempPackageFolder, string OutputFolder, string InstallerFilename);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Downloading logo from {LogoUri}")]
    private partial void LogDownloadLogo(Uri LogoUri);

    public static Uri DefaultIntuneWinAppUrl => new Uri(IntuneWinAppUtilUrl);
}