using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WingetIntune.Cli.Configuration;

namespace WingetIntune.Commands;
internal class GenerateIndexCommand : Command
{
    private const string name = "generate-index";
    private const string description = "(hidden) Generates the index.json file for the repository";
    public GenerateIndexCommand() : base(name, description)
    {
        IsHidden = true;
        AddOption(new Option<string>(new string[] { "--output-path", "-o" }, "The path to the output file") { IsRequired = true });
        AddOption(new Option<Uri>(new string[] { "--source-uri", "-s" }, () => new Uri(Winget.CommunityRepository.WingetRepository.DefaultIndexUri), "The source URI to use for the index.json file"));
        AddOption(new Option<int>(new string[] { "--timeout", "-t" }, () => 600000, "The timeout for the operation in milliseconds"));
        this.Handler = CommandHandler.Create<GenerateIndexCommandOptions, InvocationContext>(HandleCommand);
    }

    private async Task<int> HandleCommand(GenerateIndexCommandOptions options, InvocationContext context)
    {
        using var timeoutCancellation = new CancellationTokenSource(options.Timeout);
        using var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), timeoutCancellation.Token);
        var host = context.GetHost();
        var logging = host.Services.GetRequiredService<ControlableLoggingProvider>();
        
        logging.SetLogLevel(Microsoft.Extensions.Logging.LogLevel.Information);

        var repo = host.Services.GetRequiredService<Winget.CommunityRepository.WingetRepository>();
        repo.UseRespository = true;
        var packages = await repo.RefreshPackages(false, combinedCancellation.Token);
        var json = JsonSerializer.Serialize(packages);
        await File.WriteAllTextAsync(Path.GetFullPath(options.OutputPath), json, combinedCancellation.Token);
        return 0;
    }

    internal class GenerateIndexCommandOptions
    {
        public int Timeout { get; set; }
        public string OutputPath { get; set; }
        public Uri? SourceUri { get; set; }
    }
}
