using Microsoft.Graph.Beta.Models;

namespace WingetIntune.Graph;
public class Win32LobAppAutoUpdateSettings : Entity
{
    public Win32LobAppAutoUpdateSettings()
    {
        base.OdataType = "microsoft.graph.win32LobAppAutoUpdateSettings";
        AutoUpdateSupersededAppsState = "enabled";
    }

    public string? AutoUpdateSupersededAppsState
    {
        get => BackingStore?.Get<string>("autoUpdateSupersededAppsState");
        set => BackingStore?.Set("autoUpdateSupersededAppsState", value);
    }
}
