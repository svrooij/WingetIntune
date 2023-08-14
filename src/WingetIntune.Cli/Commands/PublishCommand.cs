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
    private const string description = "Publish an packaged app to Intune";

    public PublishCommand() : base(name, description)
    {
        AddCommand(new PackageImageCommand());

        AddArgument(WinGetRootCommand.IdArgument);
        AddOption(WinGetRootCommand.VersionOption);
        AddOption(new Option<string>("--package-folder", "Folder with your packaged apps")
        {
            IsRequired = true,
        });
        AddOption(new Option<string?>("--tenant", "Tenant ID to use for authentication"));
        AddOption(new Option<string?>("--username", "Username to use for authentication"));
        AddOption(new Option<string?>("--token", "Token to use against Intune (instead of tenant & username)"));
        this.Handler = CommandHandler.Create(HandleCommand);
    }

    private async Task<int> HandleCommand(PublishCommandOptions options, InvocationContext context)
    {
        var cancellationToken = context.GetCancellationToken();

        var host = context.GetHost();
        var logger = host.Services.GetRequiredService<ILogger<PackageCommand>>();
        var winget = host.Services.GetRequiredService<IWingetRepository>();
        var intuneManager = host.Services.GetRequiredService<IntuneManager>();
        var publicClient = host.Services.GetRequiredService<Internal.Msal.PublicClientAuth>();

        PackageInfo? packageInfo = null;
        if (options.Version == null)
        {
            var tempInfo = await winget.GetPackageInfoAsync(options.PackageId, null, null, cancellationToken);
            if (tempInfo == null)
            {
                //logger.LogWarning("Package {packageId} not found", options.PackageId);
                return 1;
            }
            packageInfo = await intuneManager.LoadPackageInfoFromFolder(options.PackageFolder!, options.PackageId, tempInfo.Version!, cancellationToken);
        }
        else
        {
            packageInfo = await intuneManager.LoadPackageInfoFromFolder(options.PackageFolder!, options.PackageId, options.Version, cancellationToken);
        }

        logger.LogInformation("Publishing package {packageIdentifier} {packageVersion}", options.PackageId, options.Version);
        string? token = null;
        if (options.Token != null)
        {
            token = options.Token;
        }
        else
        {
            try
            {
                var authResult = await publicClient.AccuireTokenAsync(IntuneManager.RequiredScopes, options.Tenant, options.Username, cancellationToken);
                logger.LogInformation("Got token for {username}", authResult.Account.Username);
                token = authResult.AccessToken;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get token");
                return 1;
            }
        }

        // new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions { ClientId = "d5a8a406-3b1d-4069-91cc-d76acdd812fe", TenantId = "svrooij.io", RedirectUri = new Uri("http://localhost:9005/"),  })
        var app = await intuneManager.PublishAppAsync(options.PackageFolder!, packageInfo, new Intune.IntunePublishOptions { Token = token }, cancellationToken);

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
}