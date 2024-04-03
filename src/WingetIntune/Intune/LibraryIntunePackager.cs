using Microsoft.Extensions.Logging;
using SvRooij.ContentPrep;
using WingetIntune.Interfaces;
using WingetIntune.Models;

namespace WingetIntune.Intune;

internal class LibraryIntunePackager : IIntunePackager
{
    private readonly Packager packager;
    private readonly ILogger logger;

    public LibraryIntunePackager(ILogger<Packager> logger)
    {
        this.packager = new Packager(logger);
        this.logger = logger;
    }

    public async Task<string> CreatePackage(string inputFolder, string outputFolder, string installerFilename, PackageInfo? packageInfo = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Intune package from {inputFolder}", inputFolder);
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

        var info = await packager.CreatePackage(inputFolder, Path.Combine(inputFolder, installerFilename), outputFolder, details, cancellationToken);

        return Path.GetFileNameWithoutExtension(installerFilename) + ".intunewin";
    }
}
