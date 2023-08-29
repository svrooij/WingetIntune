using Microsoft.Graph.Beta.DeviceAppManagement.MobileApps;
using Microsoft.Graph.Beta.DeviceAppManagement.MobileApps.Item;
using Microsoft.Graph.Beta.Models;

namespace WingetIntune.Graph;

public static class MobileAppsRequestBuilderExtensions
{
    private const string Win32LobType = "microsoft.graph.win32LobApp";

    public static async Task<Win32LobApp?> PostAsync(this MobileAppsRequestBuilder builder, Win32LobApp win32LobApp, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(win32LobApp);
        return await builder.PostAsync(win32LobApp, cancellationToken: cancellationToken) as Win32LobApp;
    }

    public static async Task<WinGetApp?> PostAsync(this MobileAppsRequestBuilder builder, WinGetApp wingetApp, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(wingetApp);
        return await builder.PostAsync(wingetApp, cancellationToken: cancellationToken) as WinGetApp;
    }

    public static Task<MobileAppCollectionResponse?> GetWin32Apps(this MobileAppsRequestBuilder builder, string? notesContains = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var filter = $"isof('{Win32LobType}')";
        if (!string.IsNullOrEmpty(notesContains))
        {
            filter += $" and contains(notes,'{notesContains}')";
        }
        return builder.GetAsync((config) =>
        {
            config.QueryParameters.Filter = filter;
            config.QueryParameters.Orderby = new[] { "displayName" };
        }, cancellationToken: cancellationToken);
    }
}

public static class MobileAppItemRequestBuilderExtensions
{
    public static async Task<Win32LobApp?> PatchAsync(this MobileAppItemRequestBuilder builder, Win32LobApp win32LobApp, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(win32LobApp);
        return await builder.PatchAsync(win32LobApp, cancellationToken: cancellationToken) as Win32LobApp;
    }
}
