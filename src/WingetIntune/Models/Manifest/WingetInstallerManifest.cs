namespace WingetIntune.Models.Manifest;
public class WingetInstallerManifest
{
    public string? PackageIdentifier { get; set; }
    public string? PackageVersion { get; set; }
    public string[]? Platform { get; set; }
    public string? MinimumOSVersion { get; set; }
    public WingetInstallerSwitches? InstallerSwitches { get; set; }
    public string? Scope { get; set; }
    public string? InstallerType { get; set; }
    public List<string>? InstallModes { get; set; }
    public string? UpgradeBehavior { get; set; }
    public List<string>? Commands { get; set; }
    public List<string>? FileExtensions { get; set; }
    public List<WingetInstaller>? Installers { get; set; }
    public string? ManifestType { get; set; }
    public string? ManifestVersion { get; set; }
    public string? ReleaseDate { get; set; }
}

public class WingetInstaller
{
    public string? InstallerLocale { get; set; }
    public string? Architecture { get; set; }
    public string? Scope { get; set; }
    public string? InstallerType { get; set; }
    public string? InstallerUrl { get; set; }
    public string? InstallerSha256 { get; set; }
    public WingetInstallerSwitches? InstallerSwitches { get; set; }
    public string? ProductCode { get; set; }
    public List<WingetAppsAndFeatures>? AppsAndFeaturesEntries { get; set; }
    public string? ElevationRequirement { get; set; }
    public string? InstallerFilename => InstallerUrl?.Split('/').Last();
    public InstallerContext InstallerContext => EnumParsers.ParseInstallerContext(Scope);
    public Architecture InstallerArchitecture => EnumParsers.ParseArchitecture(Architecture);
    public InstallerType ParsedInstallerType => EnumParsers.ParseInstallerType(InstallerType);

    public override string ToString()
    {
        return $"{InstallerArchitecture} {InstallerContext} {ParsedInstallerType}";
    }
}

public class WingetInstallerSwitches
{
    public string? Silent { get; set; }
    public string? SilentWithProgress { get; set; }
    public string? Interactive { get; set; }
    public string? InstallLocation { get; set; }
    public string? Log { get; set; }
    public string? Upgrade { get; set; }
    public string? Custom { get; set; }

    public override string ToString()
    {
        return Custom ?? Silent ?? SilentWithProgress ?? nameof(WingetInstallerSwitches);
    }

    public string? GetPreferred()
    {
        return Custom ?? Silent ?? SilentWithProgress; // ?? Interactive;
    }
}

public class WingetAppsAndFeatures
{
    public string? DisplayName { get; set; }
    public string? Publisher { get; set; }
    public string? DisplayVersion { get; set; }
    public string? ProductCode { get; set; }
}

internal static class WingetInstallerExtensions
{
    public static WingetInstaller? SingleOrDefault(this IList<WingetInstaller>? installers, InstallerType installerType, Architecture architecture, InstallerContext installerContext)
    {
        if (installers is null || !installers.Any()) { return null; }
        return installers.FirstOrDefault(i =>
            (i.ParsedInstallerType == installerType || installerType == InstallerType.Unknown)
            && (i.InstallerArchitecture == architecture || architecture == Architecture.Unknown)
            && (i.InstallerContext == installerContext || installerContext == InstallerContext.Unknown));
    }
}
