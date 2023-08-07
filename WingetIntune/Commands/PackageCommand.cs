using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Text;
using WingetIntune.Models;

namespace WingetIntune.Commands;

internal class PackageCommand : Command
{
    private const string name = "package";
    private const string description = "Package an app for Intune";

    private const string IntuneWinAppUtil = "IntuneWinAppUtil.exe";

    private ILogger<PackageCommand>? _logger;
    private HttpClient? _httpClient;
    private IHost? _host;

    private string packageFolder;
    private string tempPackageFolder;

    public PackageCommand() : base(name, description)
    {
        AddArgument(WinGetRootCommand.IdArgument);
        AddOption(WinGetRootCommand.VersionOption);
        AddOption(WinGetRootCommand.SourceOption);
        AddOption(new Option<string>("--temp-folder", () => Path.Combine(Path.GetTempPath(), "intunewin"), "Folder to store temporaty files")
        {
            IsRequired = true
        });
        AddOption(new Option<string>("--output-folder", "Output folder for the package")
        {
            IsRequired = false,
        });
        AddOption(new Option<Uri>("--content-prep-tool-url", () => new Uri("https://github.com/microsoft/Microsoft-Win32-Content-Prep-Tool/raw/master/IntuneWinAppUtil.exe"), "Url to download content prep tool")
        {
            IsRequired = true,
            IsHidden = true
        });
        this.Handler = CommandHandler.Create<PackageCommandOptions, InvocationContext>(HandleCommand);

    }

    private async Task<int> HandleCommand(PackageCommandOptions options, InvocationContext context)
    {
        var cancellationToken = context.GetCancellationToken();
        _host = context.GetHost();
        _logger = _host.Services.GetRequiredService<ILogger<PackageCommand>>();
        _httpClient = _host.Services.GetRequiredService<HttpClient>();
        //using var timeoutCancellation = new CancellationTokenSource(10000);
        //using var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), timeoutCancellation.Token);

        var packageInfo = await WingetManager.GetPackageInfoAsync(options.PackageId, options.Version, options.Source, cancellationToken);

        if (packageInfo == null)
        {
            Console.WriteLine($"Package {options.PackageId} not found");
            return 1;
        }

        _logger!.LogInformation("Package {PackageId} {Version} from {Source}", packageInfo.PackageId, packageInfo.Version, packageInfo.Source);

        if (packageInfo.Source == Models.PackageSource.Store)
        {
            return await HandleStorePackage(options, context, packageInfo, cancellationToken);
        }

        if (packageInfo.Source == Models.PackageSource.Winget)
        {
            return await HandleWingetPackage(options, context, packageInfo, cancellationToken);
        }

        return 10;
    }



    private static string CreateFolderForPackage(string parentFolder, PackageInfo packageInfo)
    {
        if (!Directory.Exists(parentFolder))
        {
            Directory.CreateDirectory(parentFolder);
        }
        var packageFolder = Path.Combine(parentFolder, packageInfo.PackageId!);
        if (!Directory.Exists(packageFolder))
        {
            Directory.CreateDirectory(packageFolder);
        }
        var versionFolder = Path.Combine(packageFolder, packageInfo.Version!);
        if (!Directory.Exists(versionFolder))
        {
            Directory.CreateDirectory(versionFolder);
        }
        return versionFolder;
    }

    private Task<int> HandleStorePackage(PackageCommandOptions options, InvocationContext context, PackageInfo packageInfo, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Package {options.PackageId} is a store app, not supported yet");
        return Task.FromResult(100);
    }

    private async Task<int> HandleWingetPackage(PackageCommandOptions options, InvocationContext context, PackageInfo packageInfo, CancellationToken cancellationToken)
    {
        if (options.OutputFolder == null)
        {
            Console.WriteLine($"Output folder mandatory for WinGet apps.");
            return 200;
        }

        packageFolder = CreateFolderForPackage(options.OutputFolder, packageInfo);
        tempPackageFolder = CreateFolderForPackage(options.TempFolder, packageInfo);

        await DownloadLogo(packageInfo.PackageId!, cancellationToken);
        await DownloadContentPrepTool(options.TempFolder, options.ContentPrepToolUrl, cancellationToken);

        var setupFile = Path.Combine(tempPackageFolder, packageInfo.InstallerUrl!.Segments.Last());
        _logger?.LogInformation("Downloading installer from {InstallerUrl} to {setupFile}", packageInfo.InstallerUrl, setupFile);
        await DownloadFileIfNotExists(setupFile, packageInfo.InstallerUrl, true, cancellationToken);

        switch (packageInfo.InstallerType)
        {
            case InstallerType.Msi:
                var (productCode, msiVersion) = GetMsiInfo(setupFile, _logger!);
                await WriteMsiOutput(packageInfo, productCode, msiVersion, cancellationToken);
                break;
            //case InstallerType.InnoSetup:
            //    return await HandleInnoSetupPackage(options, context, packageInfo, setupFile, packageFolder, cancellationToken);
            default:
                Console.WriteLine($"Installer type {packageInfo.InstallerType} not supported");
                return 300;
        }

        await CreatePackage(Path.Combine(options.TempFolder, IntuneWinAppUtil), packageInfo, cancellationToken);


        return 0;
    }

    private Task DownloadLogo(string packageId, CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Downloading logo for {packageId}", packageId);
        var logoPath = Path.Combine(packageFolder, "..", "logo.png");
        return DownloadFileIfNotExists(logoPath, new Uri($"https://api.winstall.app/icons/{packageId}.png"), false, cancellationToken);
    }

    private Task DownloadContentPrepTool(string tempFolder, Uri contentPrepUri, CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Downloading content prep tool from {contentPrepUri}", contentPrepUri);
        if (!Directory.Exists(tempFolder))
        {
            Directory.CreateDirectory(tempFolder);
        }

        var contentPrepToolPath = Path.Combine(tempFolder, IntuneWinAppUtil);
        return DownloadFileIfNotExists(contentPrepToolPath, contentPrepUri, true, cancellationToken);
    }
    private async Task DownloadFileIfNotExists(string path, Uri uri, bool throwOnFailure, CancellationToken cancellationToken)
    {

        if (File.Exists(path))
        {
            return;
        }

        var response = await _httpClient!.GetAsync(uri, cancellationToken);
        if (throwOnFailure && !response.IsSuccessStatusCode)
        {
            response.EnsureSuccessStatusCode();
        }
        if (response.IsSuccessStatusCode)
        {
            var imageData = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            await File.WriteAllBytesAsync(path, imageData, cancellationToken);
        }
    }

    internal static (string,string) GetMsiInfo(string setupFile, ILogger logger)
    {
        try {
            using var msi = new WixSharp.UI.MsiParser(setupFile);
            return (msi.GetProductCode(), msi.GetProductVersion());
        } catch(Exception ex)
        {
            logger?.LogError(ex, "Error getting product code from {setupFile}", setupFile);
            throw;
        }
    }

    private async Task WriteMsiOutput(PackageInfo packageInfo, string productCode, string msiVersion, CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Writing detection info for msi package {packageId} {productCode}", packageInfo.PackageId, productCode);
        var sb = new StringBuilder();
        sb.AppendFormat("Package {0} {1} from {2}\r\n", packageInfo.PackageId, packageInfo.Version, packageInfo.Source);
        sb.AppendLine();
        sb.AppendFormat("MsiProductCode={0}\r\n", productCode);
        sb.AppendFormat("MsiVersion={0}\r\n", msiVersion);

        var detectionFile = Path.Combine(packageFolder, "detection.txt");
        await File.WriteAllTextAsync(detectionFile, sb.ToString(), cancellationToken);
        sb.Clear();

        _logger?.LogInformation("Writing package readme for msi package {packageId}", packageInfo.PackageId);
        sb.AppendFormat("Package {0} {1} from {2}\r\n", packageInfo.PackageId, packageInfo.Version, packageInfo.Source);
        sb.AppendLine();
        sb.AppendLine("Install script:");
        sb.AppendFormat("msiexec /i {0} /quiet /qn\r\n", packageInfo.InstallerUrl!.Segments.Last());
        sb.AppendLine();
        sb.AppendLine("Uninstall script:");
        sb.AppendFormat("msiexec /x {0} /quiet /qn\r\n", productCode);

        var readme = Path.Combine(packageFolder, "readme.txt");
        await File.WriteAllTextAsync(readme, sb.ToString(), cancellationToken);
    } 

    private async Task CreatePackage(string contentPrepLocation, PackageInfo packageInfo, CancellationToken cancellationToken)
    {
        var setupFile = packageInfo.InstallerType == InstallerType.Msi ? packageInfo.InstallerUrl!.Segments.Last() : throw new NotImplementedException();

        var args = $"-c {tempPackageFolder} -s {setupFile} -o {packageFolder} -q";

        _logger?.LogInformation("Running content prep tool {contentPrepLocation} with args {args}", contentPrepLocation, args);
        var result = await ProcessManager.RunProcessAsync(contentPrepLocation, args, cancellationToken);
        if (result.ExitCode != 0)
        {
            _logger?.LogError("Error running content prep tool {contentPrepLocation} with args {args}", contentPrepLocation, args);
            throw new Exception($"Error running content prep tool {contentPrepLocation} with args {args}");
        }
    }
}

internal class PackageCommandOptions : WinGetRootCommand.DefaultOptions
{
    public string? OutputFolder { get; set; }
    public string TempFolder { get; set; }
    public Uri ContentPrepToolUrl { get; set; }
}
