namespace WingetIntune.Models;

public class PackageOptions
{
    public InstallerContext InstallerContext { get; init; }
    public Architecture Architecture { get; init; }
    public bool PackageScript { get; init; }

    public static PackageOptions Create() => new PackageOptions { Architecture = Architecture.X64, InstallerContext = InstallerContext.System, PackageScript = false };
}
