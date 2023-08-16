using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;

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

    private Task HandleCommand(string msiFile, InvocationContext invocationContext)
    {
        var logger = invocationContext.GetHost().Services.GetRequiredService<ILogger<MsiCommand>>();
        var absolutePath = Path.GetFullPath(msiFile);
        var (productCode, msiVersion) = IntuneManager.GetMsiInfo(absolutePath, logger);
        Console.WriteLine($"ProductCode: {productCode}");
        Console.WriteLine($"Version: {msiVersion}");

        return Task.CompletedTask;
    }
}
