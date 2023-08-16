namespace WingetIntune.Models;

public class PackageOptions
{
    private const string IntuneWinAppUtilUrl = "https://github.com/microsoft/Microsoft-Win32-Content-Prep-Tool/raw/master/IntuneWinAppUtil.exe";

    public required Uri ContentPrepUri { get; init; }
    public InstallerContext InstallerContext { get; init; }
    public Architecture Architecture { get; init; }

    public static PackageOptions Create() => new PackageOptions { Architecture = Architecture.X64, InstallerContext = InstallerContext.User, ContentPrepUri = new Uri(IntuneWinAppUtilUrl) };
}