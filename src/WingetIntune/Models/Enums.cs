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
    Msix,
    Appx,
    Exe,
    Zip,
    Inno,
    Nullsoft,
    Wix,
    Burn,
    Pwa,
    Portable,
}

public enum InstallerContext
{
    Unknown = 0,
    User,
    System,
}

public enum Architecture
{
    Unknown = 0,
    X86,
    X64,
    Arm64,
    Neutral,
}

internal static class EnumParsers
{
    public static InstallerType ParseInstallerType(string? input)
    {
        return input?.ToLowerInvariant() switch
        {
            "msi" => InstallerType.Msi,
            "msix" => InstallerType.Msix,
            "appx" => InstallerType.Appx,
            "exe" => InstallerType.Exe,
            "zip" => InstallerType.Zip,
            "inno" => InstallerType.Inno,
            "nullsoft" => InstallerType.Nullsoft,
            "wix" => InstallerType.Wix,
            "burn" => InstallerType.Burn,
            "pwa" => InstallerType.Pwa,
            "portable" => InstallerType.Portable,
            _ => InstallerType.Unknown,
        };
    }

    public static InstallerContext ParseInstallerContext(string? input)
    {
        return input?.ToLowerInvariant() switch
        {
            "user" => InstallerContext.User,
            "system" => InstallerContext.System,
            "machine" => InstallerContext.System,
            _ => InstallerContext.Unknown,
        };
    }

    public static Architecture ParseArchitecture(string? input)
    {
        return input?.ToLowerInvariant() switch
        {
            "x86" => Architecture.X86,
            "x64" => Architecture.X64,
            "arm64" => Architecture.Arm64,
            "neutral" => Architecture.Neutral,
            _ => Architecture.Unknown,
        };
    }

    public static bool IsMsi(this InstallerType installerType)
    {
        return installerType == InstallerType.Msi || installerType == InstallerType.Wix;
    }
}
