using WingetIntune.Commands;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine.Parsing;

namespace WingetIntune;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var parser = new CommandLineBuilder(new Commands.WinGetRootCommand())
            .UseHost(_ => Host.CreateDefaultBuilder(),
                           host =>
                           {
                               host.ConfigureServices(services =>
                               {
                                   services.AddHttpClient();
                                   //services.AddSingleton<IPackageManager, PackageManager>();
                               });
                           })
            .UseDefaults()
            .Build();
        return await parser.InvokeAsync(args);
        //var rootCommand = new WinGetRootCommand();
        //await rootCommand.InvokeAsync(args);
    }
}