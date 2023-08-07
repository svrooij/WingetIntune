using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WingetIntune.Commands
{
    internal class InfoCommand : Command
    {
        private const string name = "info";
        private const string description = "Show package info as json";

        public InfoCommand() : base(name, description)
        {
            AddArgument(WinGetRootCommand.IdArgument);
            AddOption(WinGetRootCommand.VersionOption);
            AddOption(WinGetRootCommand.SourceOption);
            this.Handler = CommandHandler.Create<WinGetRootCommand.DefaultOptions, InvocationContext>(HandleCommand);

        }

        private async Task<int> HandleCommand(WinGetRootCommand.DefaultOptions options, InvocationContext context)
        {
            using var timeoutCancellation = new CancellationTokenSource(10000);
            using var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), timeoutCancellation.Token);

            var packageInfo = await WingetManager.GetPackageInfoAsync(options.PackageId, options.Version, options.Source, combinedCancellation.Token);
            var jsonContext = new Models.MyJsonContext(new JsonSerializerOptions() {
                WriteIndented = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            });

            Console.WriteLine(JsonSerializer.Serialize<Models.PackageInfo>(packageInfo!, jsonContext.PackageInfo));
            return 0;
        }


    }
}
