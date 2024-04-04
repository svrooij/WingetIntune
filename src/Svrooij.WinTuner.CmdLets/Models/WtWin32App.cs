using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svrooij.WinTuner.CmdLets.Models;

/// <summary>
/// Deployed Win32App in Intune
/// </summary>
public class WtWin32App : WingetIntune.Models.IntuneApp
{
    /// <summary>
    /// Latest version of the app according to the winget repository
    /// </summary>
    public string? LatestVersion { get; set; }

    /// <summary>
    /// Is there an update available for this app
    /// </summary>
    public bool IsUpdateAvailable => LatestVersion != null && LatestVersion != CurrentVersion;
}
