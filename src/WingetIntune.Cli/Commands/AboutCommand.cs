using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Reflection;

namespace WingetIntune.Commands;

internal class AboutCommand : Command
{
    private const string name = "about";
    private const string description = "Information about this package and it's author (cross platform)";

    public AboutCommand() : base(name, description)
    {
        this.Handler = CommandHandler.Create(HandleCommand);
    }

    private Task HandleCommand(InvocationContext invocationContext)
    {
        Console.Write(header);
        Console.WriteLine("#########################################################");
        Console.WriteLine("#");
        Console.WriteLine("# Winget-Intune");
        Console.WriteLine("# By Stephan van Rooij");
        Console.WriteLine("#");
        Console.WriteLine("# command: winget-intune");
        Console.WriteLine("#");
        Console.WriteLine("# version: {0}", Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString());
        Console.WriteLine("# Repo: https://github.com/svrooij/wingetintune");
        Console.WriteLine("#");
        Console.WriteLine("# dotnet tool update --global SvRooij.Winget-Intune.Cli");
        Console.WriteLine("#");
        Console.WriteLine("#########################################################");

        return Task.CompletedTask;
    }

    private const string header = @"_ _ _ _ _  _ ____ ____ ___    _ _  _ ___ _  _ _  _ ____
| | | | |\ | | __ |___  |  __ | |\ |  |  |  | |\ | |___
|_|_| | | \| |__] |___  |     | | \|  |  |__| | \| |___

";
}
