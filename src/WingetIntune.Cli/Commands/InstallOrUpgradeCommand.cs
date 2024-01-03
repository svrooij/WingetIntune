using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;

namespace WingetIntune.Commands
{
    internal class InstallOrUpgradeCommand : Command
    {
        private const string name = "install";
        private const string description = "Installs or upgrades a package (Windows-only)";

        public InstallOrUpgradeCommand() : base(name, description)
        {
            AddArgument(WinGetRootCommand.IdArgument);
            AddOption(WinGetRootCommand.VersionOption);
            AddOption(WinGetRootCommand.SourceOption());
            //AddOption(new Option<string>(new string[] { "--arg", "-a" }, "Package arguments"));
            //AddOption(new Option<string>(new string[] { "--silent", "-q" }, "Silent install"));
            AddOption(WinGetRootCommand.ForceOption);
            //AddOption(new Option<string>(new string[] { "--log", "-l" }, "Log file"));
            //AddOption(new Option<string>(new string[] { "--timeout", "-t" }, "Timeout"));
            //AddOption(new Option<string>(new string[] { "--locale", "-c" }, "Locale"));
            //AddOption(new Option<string>(new string[] { "--override", "-o" }, "Override arguments"));
            //AddOption(new Option<string>(new string[] { "--accept-source", "-e" }, "Accept package source"));
            //AddOption(new Option<string>(new string[] { "--accept-license", "-y" }, "Accept package license"));
            //AddOption(new Option<string>(new string[] { "--hash-match", "-m" }, "Hash match"));
            //AddOption(new Option<string>(new string[] { "--hash", "-h" }, "Hash"));
            //AddOption(new Option<string>(new string[] { "--hash-algorithm", "-g" }, "Hash algorithm"));
            //AddOption(new Option<string>(new string[] { "--include-requires", "-r" }, "Include requires"));
            //AddOption(new Option<string>(new string[] { "--include-extensions", "-x" }, "Include extensions"));
            //AddOption(new Option<string>(new string[] { "--include-dependencies", "-d" }, "Include dependencies"));
            //AddOption(new Option<string>(new string[] { "--include-all-versions", "-a" }, "Include all versions"));
            //AddOption(new Option<string>(new string[] { "--accept-package-matching", "-p" }, "Accept package matching"));
            //AddOption(new Option<string>(new string[] { "--accept-package-modified", "-u" }, "Accept package modified"));
            //AddOption(new Option<string>(new string[] { "--accept-package"}))
            this.Handler = CommandHandler.Create(HandleCommand);
        }

        private async Task<int> HandleCommand(WinGetRootCommand.DefaultOptions options, InvocationContext context)
        {
            var host = context.GetHost();
            options.AdjustLogging(host);
            var logger = host.Services.GetRequiredService<ILogger<InstallOrUpgradeCommand>>();

            var winget = host.Services.GetRequiredService<IWingetRepository>();
            var installed = await winget.CheckInstalled(options.PackageId, options.Version);
            ProcessResult? result = null;
            switch (installed)
            {
                case Models.IsInstalledResult.Error:
                    logger.LogWarning("Error checking if package {PackageId} {Version} is installed", options.PackageId, options.Version);
                    return -1;

                case Models.IsInstalledResult.Installed:
                    logger.LogInformation("Package {PackageId} {Version} is already installed", options.PackageId, options.Version);
                    return 0;

                case Models.IsInstalledResult.NotInstalled:
                    result = await winget.Install(options.PackageId, options.Version, options.Source, options.Force);
                    break;

                case Models.IsInstalledResult.UpgradeAvailable:
                    result = await winget.Upgrade(options.PackageId, options.Version, options.Source, options.Force);
                    break;
            }

            if (result == null || result.ExitCode != 0)
            {
                logger.LogError("Error installing/upgrading package {PackageId} {Version}\r\n{Error}\r\n{Output}", options.PackageId, options.Version, result?.Error, result?.Output);
                return result?.ExitCode ?? 1;
            }
            logger.LogInformation("Package {PackageId} {Version} installed/upgraded successfully", options.PackageId, options.Version);
            return 0;
        }
    }
}
