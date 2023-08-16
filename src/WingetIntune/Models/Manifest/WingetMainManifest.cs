using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetIntune.Models.Manifest;
public class WingetMainManifest
{
    public string? PackageIdentifier { get; set; }
    public string? PackageVersion { get; set; }
    public string? DefaultLocale { get; set; }
    public string? ManifestType { get; set; }
    public string? ManifestVersion { get; set; }
}
