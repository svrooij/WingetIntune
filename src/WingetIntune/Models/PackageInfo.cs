namespace WingetIntune.Models;

public class PackageInfo
{
    public string? PackageId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
    public PackageSource Source { get; set; } = PackageSource.Unknown;
    public string? Publisher { get; set; }
    public Uri? HomePageUrl { get; set; }
    public Uri? PublisherUrl { get; set; }
    public Uri? SupportUrl { get; set; }
    public InstallerType InstallerType { get; set; } = InstallerType.Unknown;
    public Uri? InstallerUrl { get; set; }
    public string? Hash { get; set; }

    public static PackageInfo Parse(string wingetOutput)
    {
        var lines = wingetOutput.Split(Environment.NewLine);
        var packageInfo = new PackageInfo();

        var packageIdLine = lines.FirstOrDefault(l => l.StartsWith("Found "));
        if (packageIdLine != null)
        {
            var packageIdDetails = packageIdLine.Split(" ");
            packageInfo.Name = string.Join(" ", packageIdDetails.Skip(1).Take(packageIdDetails.Length - 2));
            packageInfo.PackageId = packageIdDetails[packageIdDetails.Length - 1].Trim('[', ']');
        }

        var versionLine = lines.FirstOrDefault(l => l.StartsWith("Version: "));
        if (versionLine != null)
        {
            packageInfo.Version = versionLine.Substring("Version: ".Length);
        }

        var publisherLine = lines.FirstOrDefault(l => l.StartsWith("Publisher: "));
        if (publisherLine != null)
        {
            packageInfo.Publisher = publisherLine.Substring("Publisher: ".Length);
        }

        var publisherUrlLine = lines.FirstOrDefault(l => l.StartsWith("Publisher Url: "));
        if (publisherUrlLine != null)
        {
            packageInfo.PublisherUrl = new Uri(publisherUrlLine.Substring("Publisher Url: ".Length));
        }

        var homepageUrlLine = lines.FirstOrDefault(l => l.StartsWith("Homepage: "));
        if (homepageUrlLine != null)
        {
            packageInfo.HomePageUrl = new Uri(homepageUrlLine.Substring("Homepage: ".Length));
        }

        var descriptionIndex = Array.FindIndex(lines, l => l.StartsWith("Description:"));
        if (descriptionIndex != -1)
        {
            packageInfo.Description = lines[descriptionIndex].Substring("Description:".Length).Trim();
            var descriptionLines = lines.Skip(descriptionIndex + 1).TakeWhile(l => l.StartsWith("  ") || l == "");
            if (descriptionLines.Any())
            {
                packageInfo.Description += $"{Environment.NewLine}" + string.Join(Environment.NewLine, descriptionLines.Select(l => l.Trim()));
            }
        }

        var supportUrlLine = lines.FirstOrDefault(l => l.StartsWith("Publisher Support Url: "));
        if (supportUrlLine != null)
        {
            packageInfo.SupportUrl = new Uri(supportUrlLine.Substring("Publisher Support Url: ".Length));
        }

        if (wingetOutput.Contains("Installer Type: msstore"))
        {
            packageInfo.Source = PackageSource.Store;
        }
        else // Assume winget
        {
            packageInfo.Source = PackageSource.Winget;

            var installerTypeLine = lines.FirstOrDefault(l => l.Contains("Installer Type:"))?.Trim();
            if (installerTypeLine != null)
            {
                var installerType = installerTypeLine.Substring("Installer Type: ".Length);
                if (installerType == "msi")
                {
                    packageInfo.InstallerType = InstallerType.Msi;
                }
                else if (installerType == "inno")
                {
                    packageInfo.InstallerType = InstallerType.InnoSetup;
                }
            }

            var installerUrlLine = lines.FirstOrDefault(l => l.Contains("Installer Url:"))?.Trim();
            if (installerUrlLine != null)
            {
                packageInfo.InstallerUrl = new Uri(installerUrlLine.Substring("Installer Url: ".Length));
            }

            var hashLine = lines.FirstOrDefault(l => l.Contains("Installer SHA256:"))?.Trim();
            if (hashLine != null)
            {
                packageInfo.Hash = hashLine.Substring("Installer SHA256: ".Length);
            }
        }

        return packageInfo;
    }
}