using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Text.Json;

namespace WingetIntune.Commands;

internal class InfoCommand : Command
{
    private const string name = "info";
    private const string description = "Show package info as json";

    public InfoCommand() : base(name, description)
    {
        AddArgument(WinGetRootCommand.IdArgument);
        AddOption(WinGetRootCommand.VersionOption);
        AddOption(WinGetRootCommand.SourceOption);
        this.Handler = CommandHandler.Create(HandleCommand);
    }

    private async Task<int> HandleCommand(WinGetRootCommand.DefaultOptions options, InvocationContext context)
    {
        using var timeoutCancellation = new CancellationTokenSource(10000);
        using var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), timeoutCancellation.Token);

        var winget = context.GetHost().Services.GetRequiredService<IWingetRepository>();
        var packageInfo = await winget.GetPackageInfoAsync(options.PackageId, options.Version, options.Source, combinedCancellation.Token);
        var jsonContext = new Models.MyJsonContext(new JsonSerializerOptions()
        {
            WriteIndented = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        });

        Console.WriteLine(JsonSerializer.Serialize<Models.PackageInfo>(packageInfo!, jsonContext.PackageInfo));
        return 0;
    }
}