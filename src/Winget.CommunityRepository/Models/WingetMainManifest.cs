﻿namespace Winget.CommunityRepository.Models;

public partial class WingetMainManifest
{
    public string? PackageIdentifier { get; set; }
    public string? PackageVersion { get; set; }
    public string? DefaultLocale { get; set; }
    public string? ManifestType { get; set; }
    public string? ManifestVersion { get; set; }
}
