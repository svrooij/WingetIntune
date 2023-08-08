namespace WingetIntune.Models;

public enum IsInstalledResult
{
    Error = -1,
    Installed,
    NotInstalled,
    UpgradeAvailable
}

public enum PackageSource
{
    Unknown = 0,
    Store,
    Winget,
}

public enum InstallerType
{
    Unknown = 0,
    Msi,
    InnoSetup,
}