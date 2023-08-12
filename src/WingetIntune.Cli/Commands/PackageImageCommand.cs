using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;

namespace WingetIntune.Commands;

internal class PackageImageCommand : Command
{
    private const string name = "image";
    private const string description = "Package an app image for Intune";

    private static Argument<string> ImagePathArgument = new Argument<string>("image-path", "Path to the image to package")
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
        var cancellationToken = context.GetCancellationToken();

        var path = Path.GetFullPath(imagePath);
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);

        var output = System.Convert.ToBase64String(bytes);

        context.Console.Out.Write(output);

        return 0;
    }
}