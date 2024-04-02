using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Text.Json;
using ConsoleTables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WingetIntune.Models;

namespace WingetIntune.Commands;

internal class UpdateListCommand : Command
{
    private const string name = "list";
    private const string description = "Show the list of published apps in Intune (cross platform)";

    public UpdateListCommand() : base(name, description)
    {
        AddOption(PublishCommand.TenantOption);
        AddOption(PublishCommand.UsernameOption);
        AddOption(PublishCommand.TokenOption);
        this.Handler = CommandHandler.Create(HandleCommandAsync);
    }

    private async Task<int> HandleCommandAsync(UpdateCommandOptions options, InvocationContext context)
    {
        var cancellationToken = context.GetCancellationToken();

        var host = context.GetHost();
        options.AdjustLogging(host);

        var logger = host.Services.GetRequiredService<ILogger<UpdateListCommand>>();
        logger.LogInformation("Getting list of published apps");
        var repo = host.Services.GetRequiredService<Winget.CommunityRepository.WingetRepository>();
        var intuneManager = host.Services.GetRequiredService<IntuneManager>();

        var apps = await intuneManager.GetPublishedAppsAsync(options.GetPublishOptions(), cancellationToken);

        var result = await GetUpdateAbleAppsAsync(apps, repo, cancellationToken);
        if (options.Json)
        {
            Console.WriteLine(JsonSerializer.Serialize(result!));
            return 0;
        }
        var table = new ConsoleTable("PackageId", "Version", "LatestVersion", "UpdateAvailable");
        foreach (var app in result.OrderByDescending(a => a.IsUpdateAvailable).ThenBy(a => a.PackageId))
        {
            table.AddRow(app.PackageId, app.Version, app.LatestVersion, app.IsUpdateAvailable);
        }
        table.Write(Format.Minimal);
        return 0;
    }

    private static async Task<IEnumerable<UpdateAbleIntuneApp>> GetUpdateAbleAppsAsync(IEnumerable<IntuneApp> apps, Winget.CommunityRepository.WingetRepository repo, CancellationToken cancellationToken)
    {
        var result = new List<UpdateAbleIntuneApp>();
        foreach (var app in apps)
        {
            var latestVersion = await repo.GetLatestVersion(app.PackageId, cancellationToken);
            result.Add(new UpdateAbleIntuneApp
            {
                GraphId = app.GraphId,
                PackageId = app.PackageId,
                Name = app.Name,
                Version = app.Version,
                LatestVersion = latestVersion,
            });
        }
        return result;
    }
}
