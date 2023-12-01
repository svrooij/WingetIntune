using Microsoft.Extensions.Logging;
using SvRooij.ContentPrep;
using WingetIntune.Interfaces;
using WingetIntune.Models;

namespace WingetIntune.Intune;

internal class LibraryIntunePackager : IIntunePackager
{
    private readonly Packager packager;

    public LibraryIntunePackager(ILogger<Packager> logger)
    {
        this.packager = new Packager(logger);
    }

    public Task CreatePackage(string inputFolder, string outputFolder, string installerFilename, PackageInfo? packageInfo = null, CancellationToken cancellationToken = default)
    {
        var details = new SvRooij.ContentPrep.Models.ApplicationDetails
        {
            Name = packageInfo?.DisplayName ?? packageInfo?.PackageIdentifier,
        };
        if (packageInfo?.MsiProductCode != null && packageInfo.MsiVersion != null)
        {
            details.MsiInfo = new SvRooij.ContentPrep.Models.MsiInfo
            {
                MsiProductCode = packageInfo.MsiProductCode,
                MsiProductVersion = packageInfo.MsiVersion,
            };
        }

        return packager.CreatePackage(inputFolder, Path.Combine(inputFolder, installerFilename), outputFolder, details, cancellationToken);
    }
}
