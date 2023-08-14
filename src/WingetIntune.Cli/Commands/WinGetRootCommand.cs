using System.CommandLine;

namespace WingetIntune.Commands;

internal class WinGetRootCommand : RootCommand
{
    internal static Argument<string> IdArgument { get; } = new Argument<string>("packageId", "Package identifier");
    internal static Option<string> VersionOption { get; } = new Option<string>(new string[] { "--version", "-v" }, "Package Version");
    internal static Option<string> SourceOption { get; } = new Option<string>(new string[] { "--source", "-s" }, "Package source");
    internal static Option<bool> ForceOption { get; } = new Option<bool>(new string[] { "--force", "-f" }, "Force install");

    public WinGetRootCommand()
    {
        Description = "Enhanced Winget CLI for automations";
        AddCommand(new PackageCommand());
        AddCommand(new PublishCommand());
        AddCommand(new InstallOrUpgradeCommand());
        AddCommand(new CheckCommand());
        AddCommand(new InfoCommand());
        AddCommand(new MsiCommand());
        AddCommand(new AboutCommand());
    }

    internal class DefaultOptions
    {
        public string PackageId { get; set; }
        public string? Version { get; set; }
        public string? Source { get; set; }
        public bool Force { get; set; }
    }
}