using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetIntune.Models.Manifest;
public class WingetLocalizedManifest
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
}
