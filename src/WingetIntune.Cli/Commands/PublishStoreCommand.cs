using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using WingetIntune.Intune;

namespace WingetIntune.Commands;

internal class PublishStoreCommand : Command
{
    private const string name = "store";
    private const string description = "Publish a Microsoft Store app to Intune";

    public PublishStoreCommand() : base(name, description)
    {
        AddOption(new Option<string>(new[] { "--id" }, "Package id of the app in the store (use winget search)")
        {
            IsRequired = false
        });

        AddOption(new Option<string>(new[] { "--search" }, "App name in the store, it publishes the first result")
        {
            IsRequired = false
        });

        AddOption(PublishCommand.TenantOption);
        AddOption(PublishCommand.UsernameOption);
        AddOption(PublishCommand.TokenOption);
        AddOption(PublishCommand.CategoryOption);
        AddOption(PublishCommand.AvailableForOption);
        AddOption(PublishCommand.RequiredForOption);
        AddOption(PublishCommand.UninstallForOption);

        this.Handler = CommandHandler.Create(HandleCommand);
    }

    private async Task<int> HandleCommand(PublishStoreCommandOptions options, InvocationContext context)
    {
        var cancellationToken = context.GetCancellationToken();

        var host = context.GetHost();
        options.AdjustLogging(host);
        var logger = host.Services.GetRequiredService<ILogger<PublishStoreCommand>>();
        var intuneManager = host.Services.GetRequiredService<IntuneManager>();

        Microsoft.Graph.Beta.Models.WinGetApp? app = null;

        var publishOptions = new IntunePublishOptions
        {
            Tenant = options.Tenant,
            Username = options.Username,
            Token = options.Token,
            Categories = options.Category,
            AvailableFor = options.Available,
            RequiredFor = options.Required,
            UninstallFor = options.Uninstall
        };

        if (string.IsNullOrEmpty(options.Id))
        {
            if (string.IsNullOrEmpty(options.Search))
            {
                logger.LogError("Either --id or --search must be specified");
                return 1;
            }
            logger.LogInformation("Publishing MSStore app by name {search}", options.Search);
            app = await intuneManager.PublishStoreAppAsync(publishOptions, searchString: options.Search, cancellationToken: cancellationToken);
        }
        else
        {
            logger.LogInformation("Publishing MSStore app by id {id}", options.Id);
            app = await intuneManager.PublishStoreAppAsync(publishOptions, packageId: options.Id, cancellationToken: cancellationToken);
        }

        logger.LogInformation("App {packageIdentifier} created in Azure {appId}", app.PackageIdentifier, app.Id); ;

        return 0;
    }
}

internal class PublishStoreCommandOptions : WinGetRootCommand.DefaultOptions
{
    public string? Id { get; set; }
    public string? Search { get; set; }
    public string? Tenant { get; set; }
    public string? Username { get; set; }
    public string? Token { get; set; }
    public string[] Category { get; set; } = Array.Empty<string>();
    public string[] Available { get; set; } = Array.Empty<string>();
    public string[] Required { get; set; } = Array.Empty<string>();
    public string[] Uninstall { get; set; } = Array.Empty<string>();
}
