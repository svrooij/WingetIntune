using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using WingetIntune.Models;

namespace WingetIntune.Commands;

internal class PackageCommand : Command
{
    private const string name = "package";
    private const string description = "Package an app for Intune (cross platform)";

    internal static readonly Option<string> TempFolderOption = new Option<string>("--temp-folder", () => Path.Combine(Path.GetTempPath(), "intunewin"), "Folder to store temporaty files")
    {
        IsHidden = true,
        IsRequired = true,
    };

    internal static Option<string> GetPackageFolderOption(bool isRequired = false, bool isHidden = false) => new Option<string>("--package-folder", "Folder with your packaged apps")
    {
        IsRequired = isRequired,
        IsHidden = isHidden,
    };

    internal static Option<Architecture> GetArchitectureOption(bool isHidden = false) => new Option<Architecture>("--architecture", () => Architecture.X64, "Architecture to package for")
    {
        IsHidden = isHidden,
    };

    internal static Option<InstallerContext> GetInstallerContextOption(bool isHidden = false) => new Option<InstallerContext>("--installer-context", () => InstallerContext.System, "Installer context to use")
    {
        IsHidden = isHidden,
    };

    internal static Option<bool?> GetPackageAsScriptOption(bool isHidden = false) => new Option<bool?>("--package-script", () => null, "Package just a winget script, not the installer itself")
    {
        IsHidden = isHidden,
    };

    internal static readonly Option<bool> UseWingetOption = new Option<bool>("--use-winget", "Use winget to get the package information, instead of the faster package index")
    {
        IsHidden = false,
    };

    public PackageCommand() : base(name, description)
    {
        AddCommand(new PackageImageCommand());

        AddArgument(WinGetRootCommand.IdArgument);
        AddOption(WinGetRootCommand.VersionOption);
        AddOption(WinGetRootCommand.SourceOption("winget"));
        AddOption(TempFolderOption);
        AddOption(GetPackageFolderOption(isRequired: true));
        AddOption(GetArchitectureOption(isHidden: false));
        AddOption(GetInstallerContextOption(isHidden: false));
        AddOption(GetPackageAsScriptOption(isHidden: false));
        AddOption(UseWingetOption);
        this.Handler = CommandHandler.Create(HandleCommand);
    }

    private async Task<int> HandleCommand(PackageCommandOptions options, InvocationContext context)
    {
        var cancellationToken = context.GetCancellationToken();

        var host = context.GetHost();
        options.AdjustLogging(host);
        var logger = host.Services.GetRequiredService<ILogger<PackageCommand>>();
        var repo = host.Services.GetRequiredService<Winget.CommunityRepository.WingetRepository>();
        var winget = host.Services.GetRequiredService<IWingetRepository>();
        var intuneManager = host.Services.GetRequiredService<IntuneManager>();

        if (options.Version is null && options.Source == "winget" && !options.UseWinget)
        {
            logger.LogInformation("Getting latest version for {PackageId}", options.PackageId);
            options.PackageId = (await repo.GetPackageId(options.PackageId, cancellationToken))!;
            options.Version = await repo.GetLatestVersion(options.PackageId, cancellationToken);
        }

        var packageInfo = await winget.GetPackageInfoAsync(options.PackageId, options.Version, options.Source, cancellationToken);

        if (packageInfo == null)
        {
            logger.LogWarning("Package {packageId} not found", options.PackageId);
            return 1;
        }

        logger.LogInformation("Package {PackageId} {Version} from {Source}", packageInfo.PackageIdentifier, packageInfo.Version, packageInfo.Source);

        if (packageInfo.Source == Models.PackageSource.Winget)
        {
            await intuneManager.GenerateInstallerPackage(options.TempFolder,
                options.PackageFolder!,
                packageInfo,
                new PackageOptions { Architecture = options.Architecture, InstallerContext = options.InstallerContext, PackageScript = options.PackageScript == true },
                cancellationToken);

            return 0;
        }

        throw new NotImplementedException("Only WinGet packages are supported at this time");
    }
}

internal class PackageCommandOptions : WinGetRootCommand.DefaultOptions
{
    public string? PackageFolder { get; set; }
    public string TempFolder { get; set; }
    public bool UseWinget { get; set; }
    public InstallerContext InstallerContext { get; set; }
    public Architecture Architecture { get; set; }
    public bool? PackageScript { get; set; }
}
