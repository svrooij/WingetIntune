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
    private const string description = "Package an app for Intune";

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

    internal static Option<InstallerContext> GetInstallerContextOption(bool isHidden = false) => new Option<InstallerContext>("--installer-context", () => InstallerContext.User, "Installer context to use")
    {
        IsHidden = isHidden,
    };

    public PackageCommand() : base(name, description)
    {
        AddCommand(new PackageImageCommand());

        AddArgument(WinGetRootCommand.IdArgument);
        AddOption(WinGetRootCommand.VersionOption);
        AddOption(WinGetRootCommand.SourceOption);
        AddOption(TempFolderOption);
        AddOption(GetPackageFolderOption(isRequired: true));
        AddOption(GetArchitectureOption(isHidden: false));
        AddOption(GetInstallerContextOption(isHidden: false));
        this.Handler = CommandHandler.Create(HandleCommand);
    }

    private async Task<int> HandleCommand(PackageCommandOptions options, InvocationContext context)
    {
        var cancellationToken = context.GetCancellationToken();

        var host = context.GetHost();
        options.AdjustLogging(host);
        var logger = host.Services.GetRequiredService<ILogger<PackageCommand>>();
        var winget = host.Services.GetRequiredService<IWingetRepository>();
        var intuneManager = host.Services.GetRequiredService<IntuneManager>();

        var packageInfo = await winget.GetPackageInfoAsync(options.PackageId, options.Version, options.Source, cancellationToken);

        if (packageInfo == null)
        {
            logger.LogWarning("Package {packageId} not found", options.PackageId);
            return 1;
        }

        logger.LogInformation("Package {PackageId} {Version} from {Source}", packageInfo.PackageIdentifier, packageInfo.Version, packageInfo.Source);

        if (packageInfo.Source == Models.PackageSource.Winget)
        {
            if (packageInfo.Installers?.Any() != true)
            {
                // Load the installers from the manifest
                // This is done automatically if the `--source winget` option is used and the version is specified
                packageInfo = await winget.GetPackageInfoAsync(packageInfo.PackageIdentifier!, packageInfo.Version!, "winget", cancellationToken);
            }
            await intuneManager.GenerateInstallerPackage(options.TempFolder,
                options.PackageFolder!,
                packageInfo,
                new PackageOptions { Architecture = options.Architecture, InstallerContext = options.InstallerContext },
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
    public InstallerContext InstallerContext { get; set; }
    public Architecture Architecture { get; set; }
}
