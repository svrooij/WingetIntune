using System.CommandLine;
using WingetIntune.Models;

namespace WingetIntune.Commands;

internal class UpdateCommand : Command
{
    private const string name = "update";
    private const string description = "Update a published app in Intune (cross platform)";

    public UpdateCommand() : base(name, description)
    {
        AddCommand(new UpdateListCommand());
    }
}

internal class UpdateCommandOptions : WinGetRootCommand.DefaultOptions
{
    public string? PackageFolder { get; set; }
    public string? Tenant { get; set; }
    public string? Username { get; set; }
    public string? Token { get; set; }

    internal Intune.IntunePublishOptions GetPublishOptions()
    {
        return new Intune.IntunePublishOptions
        {
            Tenant = Tenant,
            Username = Username,
            Token = Token
        };
    }
}

internal class UpdateAbleIntuneApp : IntuneApp
{
    public string? LatestVersion { get; set; }
    public bool IsUpdateAvailable => LatestVersion != null && LatestVersion != Version;
}
