using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;

namespace WingetIntune.Commands;

internal class PackageCommand : Command
{
    private const string name = "package";
    private const string description = "Package an app for Intune";

    public PackageCommand() : base(name, description)
    {
        AddCommand(new PackageImageCommand());

        AddArgument(WinGetRootCommand.IdArgument);
        AddOption(WinGetRootCommand.VersionOption);
        AddOption(WinGetRootCommand.SourceOption);
        AddOption(new Option<string>("--temp-folder", () => Path.Combine(Path.GetTempPath(), "intunewin"), "Folder to store temporaty files")
        {
            IsRequired = true
        });
        AddOption(new Option<string>("--package-folder", "Folder for the packaged apps")
        {
            IsRequired = false,
        });
        AddOption(new Option<Uri>("--content-prep-tool-url", () => IntuneManager.DefaultIntuneWinAppUrl, "Url to download content prep tool")
        {
            IsRequired = true,
            IsHidden = true
        });
        this.Handler = CommandHandler.Create(HandleCommand);
    }

    private async Task<int> HandleCommand(PackageCommandOptions options, InvocationContext context)
    {
        var cancellationToken = context.GetCancellationToken();

        var host = context.GetHost();
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
            await intuneManager.GenerateInstallerPackage(options.TempFolder, options.PackageFolder!, packageInfo, options.ContentPrepToolUrl, cancellationToken);

            return 0;
        }

        throw new NotImplementedException("Only WinGet packages are supported at this time");
    }
}

internal class PackageCommandOptions : WinGetRootCommand.DefaultOptions
{
    public string? PackageFolder { get; set; }
    public string TempFolder { get; set; }
    public Uri ContentPrepToolUrl { get; set; }
}