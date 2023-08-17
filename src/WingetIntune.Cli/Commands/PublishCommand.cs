using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using WingetIntune.Models;

namespace WingetIntune.Commands;

internal class PublishCommand : Command
{
    private const string name = "publish";
    private const string description = "Publish a packaged app to Intune";

    internal static readonly Option<string?> TenantOption = new Option<string?>("--tenant", "Tenant ID to use for authentication");
    internal static readonly Option<string?> UsernameOption = new Option<string?>("--username", "Username to use for authentication");
    internal static readonly Option<string?> TokenOption = new Option<string?>("--token", "Token to use against Intune (instead of tenant & username)");

    public PublishCommand() : base(name, description)
    {
        AddCommand(new PublishStoreCommand());
        AddCommand(new PackageImageCommand());

        AddArgument(WinGetRootCommand.IdArgument);
        AddOption(WinGetRootCommand.VersionOption);
        AddOption(PackageCommand.GetPackageFolderOption(isRequired: true, isHidden: false));
        AddOption(TenantOption);
        AddOption(UsernameOption);
        AddOption(TokenOption);
        AddOption(PackageCommand.TempFolderOption);
        AddOption(new Option<bool>("--auto-package", "Automatically package the app if it's not found in the package folder") { IsHidden = true });
        AddOption(PackageCommand.GetArchitectureOption(isHidden: true));
        AddOption(PackageCommand.GetInstallerContextOption(isHidden: true));
        AddOption(PackageCommand.ContentPrepToolUriOption);
        this.Handler = CommandHandler.Create(HandleCommand);
    }

    private async Task<int> HandleCommand(PublishCommandOptions options, InvocationContext context)
    {
        var cancellationToken = context.GetCancellationToken();

        var host = context.GetHost();
        var logger = host.Services.GetRequiredService<ILogger<PackageCommand>>();
        var winget = host.Services.GetRequiredService<IWingetRepository>();
        var intuneManager = host.Services.GetRequiredService<IntuneManager>();

        PackageInfo? packageInfo = null;
        if (options.Version == null)
        {
            var tempInfo = await winget.GetPackageInfoAsync(options.PackageId, null, null, cancellationToken);
            if (tempInfo == null)
            {
                //logger.LogWarning("Package {packageId} not found", options.PackageId);
                return 1;
            }
            if (options.AutoPackage && tempInfo.Source == PackageSource.Winget)
            {
                tempInfo = await winget.GetPackageInfoAsync(tempInfo.PackageIdentifier!, tempInfo.Version, tempInfo.Source.ToString().ToLower(), cancellationToken);
                await intuneManager.GenerateInstallerPackage(options.TempFolder,
                options.PackageFolder!,
                tempInfo,
                new PackageOptions { Architecture = options.Architecture, ContentPrepUri = options.ContentPrepToolUrl, InstallerContext = options.InstallerContext },
                cancellationToken);
            }
            packageInfo = tempInfo.Source == PackageSource.Store
                ? tempInfo
                : await intuneManager.LoadPackageInfoFromFolder(options.PackageFolder!, options.PackageId, tempInfo.Version!, cancellationToken);
        }
        else
        {
            packageInfo = await intuneManager.LoadPackageInfoFromFolder(options.PackageFolder!, options.PackageId, options.Version, cancellationToken);
        }

        logger.LogInformation("Publishing package {packageIdentifier} {packageVersion}", options.PackageId, options.Version);

        var publishOptions = new Intune.IntunePublishOptions { Username = options.Username, Tenant = options.Tenant, Token = options.Token };

        var app = packageInfo.Source == PackageSource.Store
            ? await intuneManager.PublishStoreAppAsync(publishOptions, packageInfo.PackageIdentifier, cancellationToken: cancellationToken)
            : await intuneManager.PublishAppAsync(options.PackageFolder!, packageInfo, publishOptions, cancellationToken);

        logger.LogInformation("App {packageIdentifier} {packageVersion} created in Azure {appId}", packageInfo.PackageIdentifier, packageInfo.Version, app.Id); ;

        return 0;
    }
}

internal class PublishCommandOptions : WinGetRootCommand.DefaultOptions
{
    public string? PackageFolder { get; set; }
    public string? Tenant { get; set; }
    public string? Username { get; set; }
    public string? Token { get; set; }
    public bool AutoPackage { get; set; }
    public string TempFolder { get; set; }
    public Uri ContentPrepToolUrl { get; set; }
    public InstallerContext InstallerContext { get; set; }
    public Architecture Architecture { get; set; }
}
