using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetIntune.Commands
{
    internal class CheckCommand : Command
    {
        private const string name = "check";
        private const string description = "Check if a specific version in installed";

        public CheckCommand() : base(name, description)
        {
            AddArgument(WinGetRootCommand.IdArgument);
            AddOption(WinGetRootCommand.VersionOption);
            AddOption(WinGetRootCommand.SourceOption);


            this.Handler = CommandHandler.Create<WinGetRootCommand.DefaultOptions, InvocationContext>(HandleCommand);

        }

        private async Task<int> HandleCommand(WinGetRootCommand.DefaultOptions options, InvocationContext context)
        {
            var installed = await WingetManager.CheckInstalled(options.PackageId, options.Version, context.GetCancellationToken());
            switch (installed)
            {
                case Models.IsInstalledResult.Error:
                    Console.WriteLine($"Error checking if package {options.PackageId} {options.Version} is installed");
                    return -1;
                case Models.IsInstalledResult.Installed:
                    Console.WriteLine($"Package {options.PackageId} {options.Version} is already installed");
                    return 0;
                case Models.IsInstalledResult.NotInstalled:
                    Console.WriteLine($"Package {options.PackageId} {options.Version} is not installed");
                    return 100;
                case Models.IsInstalledResult.UpgradeAvailable:
                    Console.WriteLine($"Package {options.PackageId} has a different version installed");
                    return 101;
            }

            throw new NotImplementedException("This should never be reached.");
        }


    }
}
