namespace Svrooij.WinTuner.CmdLets.Models;

/// <summary>
/// Information about an MSI file
/// </summary>
public class MsiInfo
{
    /// <summary>
    /// The path to the MSI file
    /// </summary>
    public string? Path { get; set; }
    /// <summary>
    /// The product code of the MSI file
    /// </summary>
    public string? ProductCode { get; set; }

    /// <summary>
    /// The version of the MSI file
    /// </summary>
    public string? ProductVersion { get; set; }
}
