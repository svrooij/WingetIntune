namespace WingetIntune.Models;
public class IntuneApp
{
    /// <summary>
    /// Package ID from the winget manifest
    /// </summary>
    public required string PackageId { get; set; }

    /// <summary>
    /// Display name of the app
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Current version of the app
    /// </summary>
    public required string CurrentVersion { get; set; }

    /// <summary>
    /// Graph ID of the app
    /// </summary>
    public required string GraphId { get; set; }

    /// <summary>
    /// The total number of apps this app is directly or indirectly superseded by.
    /// </summary>
    public int? SupersededAppCount { get; set; }

    /// <summary>
    /// The total number of apps this app directly or indirectly supersedes
    /// </summary>
    public int? SupersedingAppCount { get; set; }

}
