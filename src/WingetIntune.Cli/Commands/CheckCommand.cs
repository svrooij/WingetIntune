using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;

namespace WingetIntune.Commands;

internal class CheckCommand : Command
{
    private const string name = "check";
    private const string description = "Check if a specific version in installed";

    public CheckCommand() : base(name, description)
    {
        AddArgument(WinGetRootCommand.IdArgument);
        AddOption(WinGetRootCommand.VersionOption);
        AddOption(WinGetRootCommand.SourceOption());

        this.Handler = CommandHandler.Create(HandleCommand);
    }

    private async Task<int> HandleCommand(WinGetRootCommand.DefaultOptions options, InvocationContext context)
    {
        var winget = context.GetHost().Services.GetRequiredService<IWingetRepository>();
        var installed = await winget.CheckInstalled(options.PackageId, options.Version, context.GetCancellationToken());
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
