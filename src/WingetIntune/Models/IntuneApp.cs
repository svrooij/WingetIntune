namespace WingetIntune.Models;
public class IntuneApp
{
    /// <summary>
    /// Package ID from the winget manifest
    /// </summary>
    public string PackageId { get; set; }

    /// <summary>
    /// Display name of the app
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Current version of the app
    /// </summary>
    public string CurrentVersion { get; set; }

    /// <summary>
    /// Graph ID of the app
    /// </summary>
    public string GraphId { get; set; }

    /// <summary>
    /// The total number of apps this app is directly or indirectly superseded by.
    /// </summary>
    public int? SupersededAppCount { get; set; }

    /// <summary>
    /// The total number of apps this app directly or indirectly supersedes
    /// </summary>
    public int? SupersedingAppCount { get; set; }

    /// <summary>
    /// The installer context
    /// </summary>
    /// <remarks>
    /// You should probably not update an app with a different installer context
    /// </remarks>
    public InstallerContext InstallerContext { get; set; }

    /// <summary>
    /// The architecture of the app
    /// </summary>
    /// <remarks>
    /// You should probably not update an app with a different architecture
    /// </remarks>
    public Architecture Architecture { get; set; }

}
