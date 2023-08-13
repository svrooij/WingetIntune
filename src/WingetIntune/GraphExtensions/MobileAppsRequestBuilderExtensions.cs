using Microsoft.Graph.Beta.DeviceAppManagement.MobileApps;
using Microsoft.Graph.Beta.Models;
using Microsoft.Graph.Beta.Models.ODataErrors;
using Microsoft.Kiota.Abstractions.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetIntune.GraphExtensions;
internal static class MobileAppsRequestBuilderExtensions
{
    public static async Task<Win32LobApp?> PostAsync(this MobileAppsRequestBuilder builder, Win32LobApp win32LobApp, CancellationToken cancellationToken)
    {
        return await builder.PostAsync(win32LobApp, cancellationToken: cancellationToken) as Win32LobApp;
    }

    public static async Task<Win32LobApp?> PatchAsync(this MobileAppsRequestBuilder builder, Win32LobApp win32LobApp, CancellationToken cancellationToken)
    {
        return await builder.PatchAsync(win32LobApp, cancellationToken: cancellationToken) as Win32LobApp;
    }


}
