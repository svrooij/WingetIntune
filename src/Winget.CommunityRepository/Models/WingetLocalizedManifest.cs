﻿namespace Winget.CommunityRepository.Models;

public partial class WingetLocalizedManifest
{
    public string? PackageIdentifier { get; set; }
    public string? PackageVersion { get; set; }
    public string? PackageLocale { get; set; }
    public string? Publisher { get; set; }
    public Uri? PublisherUrl { get; set; }
    public Uri? PublisherSupportUrl { get; set; }
    public Uri? PrivacyUrl { get; set; }
    public string? Author { get; set; }
    public string? PackageName { get; set; }
    public Uri? PackageUrl { get; set; }
    public Uri? PurchaseUrl { get; set; }
    public string? License { get; set; }
    public Uri? LicenseUrl { get; set; }
    public string? Copyright { get; set; }
    public Uri? CopyrightUrl { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? Moniker { get; set; }
    public List<string>? Tags { get; set; }
    public string? ReleaseNotesUrl { get; set; }
    public string? ManifestType { get; set; }
    public string? ManifestVersion { get; set; }

    public WingetLocalizedManifestDocumentation[]? Documentations { get; set; }
}

public partial class WingetLocalizedManifestDocumentation
{
    public string? DocumentLabel { get; set; }
    public Uri? DocumentUrl { get; set; }
}
