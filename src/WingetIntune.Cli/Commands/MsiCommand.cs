using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using WingetIntune.Cli.Configuration;

namespace WingetIntune.Commands;

internal class MsiCommand : Command
{
    private const string name = "msi";
    private const string description = "Extract info from MSI file";

    private Argument<string> msiFileArgument = new Argument<string>("msiFile", "Path to MSI file");

    public MsiCommand() : base(name, description)
    {
        AddArgument(msiFileArgument);
        this.Handler = CommandHandler.Create(HandleCommand);
    }

    private Task HandleCommand(string msiFile, bool json, bool verbose, InvocationContext invocationContext)
    {
        var host = invocationContext.GetHost();
        var logger = host.Services.GetRequiredService<ILogger<MsiCommand>>();
        if (json || verbose)
        {
            var logConfiguration = host.Services.GetRequiredService<ControlableLoggingProvider>();
            if (json)
            {
                logConfiguration.SetOutputFormat("json");
            }
            if (verbose)
            {
                logConfiguration.SetVerbose();
            }
        }
        var absolutePath = Path.GetFullPath(msiFile);
        var (productCode, msiVersion) = IntuneManager.GetMsiInfo(absolutePath, logger);
        logger.LogInformation($"ProductCode: {productCode}");
        logger.LogInformation($"Version: {msiVersion}");

        return Task.CompletedTask;
    }
}
