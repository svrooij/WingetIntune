using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using WingetIntune.Cli.Configuration;

namespace WingetIntune.Commands;

internal class PackageImageCommand : Command
{
    private const string name = "image";
    private const string description = "Convert an image to base64, to upload to Intune.";

    private static Argument<string> ImagePathArgument = new Argument<string>("image-path", "Path to the image to convert")
    {
        Arity = ArgumentArity.ExactlyOne
    };

    public PackageImageCommand() : base(name, description)
    {
        AddArgument(ImagePathArgument);

        this.Handler = CommandHandler.Create(HandleCommand);
    }

    private async Task<int> HandleCommand(string imagePath, InvocationContext context)
    {
        var host = context.GetHost();
        var loggingProvider = host.Services.GetRequiredService<ControlableLoggingProvider>();
        loggingProvider.SetOutputFormat("json");
        loggingProvider.SetLogLevel(Microsoft.Extensions.Logging.LogLevel.Warning);
        var cancellationToken = context.GetCancellationToken();

        var path = Path.GetFullPath(imagePath);
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);

        var output = System.Convert.ToBase64String(bytes);

        context.Console.Out.Write(output);

        return 0;
    }
}
