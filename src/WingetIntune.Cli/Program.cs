using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;

namespace WingetIntune;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var parser = new CommandLineBuilder(new Commands.WinGetRootCommand())
            .UseHost(_ => Host.CreateDefaultBuilder(),
                           host =>
                           {
                               host.ConfigureServices(services =>
                               {
                                   services.AddWingetServices();
                               });
                           })
            .UseDefaults()
            .Build();
        return await parser.InvokeAsync(args);
    }
}