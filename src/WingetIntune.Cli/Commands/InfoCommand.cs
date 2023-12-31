﻿using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Text.Json;
using WingetIntune.Cli.Configuration;
using WingetIntune.Models;

namespace WingetIntune.Commands;

internal class InfoCommand : Command
{
    private const string name = "info";
    private const string description = "Show package info as json (Windows-only)";

    public InfoCommand() : base(name, description)
    {
        AddArgument(WinGetRootCommand.IdArgument);
        AddOption(WinGetRootCommand.VersionOption);
        AddOption(WinGetRootCommand.SourceOption("winget"));
        this.Handler = CommandHandler.Create(HandleCommand);
    }

    private async Task<int> HandleCommand(WinGetRootCommand.DefaultOptions options, InvocationContext context)
    {
        using var timeoutCancellation = new CancellationTokenSource(60000);
        using var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), timeoutCancellation.Token);
        var host = context.GetHost();
        var logging = host.Services.GetRequiredService<ControlableLoggingProvider>();
        logging.SetOutputFormat("json");
        logging.SetLogLevel(Microsoft.Extensions.Logging.LogLevel.Warning);

        if (options.Version is null)
        {
            var repo = new Winget.CommunityRepository.WingetRepository(host.Services.GetRequiredService<HttpClient>());
            options.Version = await repo.GetLatestVersion(options.PackageId, combinedCancellation.Token);
        }

        var winget = host.Services.GetRequiredService<IWingetRepository>();
        var packageInfo = await winget.GetPackageInfoAsync(options.PackageId, options.Version, options.Source, combinedCancellation.Token);

        Console.WriteLine(JsonSerializer.Serialize(packageInfo!, MyJsonContext.Default.PackageInfo));
        return 0;
    }
}
