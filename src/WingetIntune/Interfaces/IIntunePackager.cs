using WingetIntune.Models;

namespace WingetIntune.Interfaces;

public interface IIntunePackager
{
    Task<string> CreatePackage(string inputFolder, string outputFolder, string installerFilename, PackageInfo? packageInfo = null, CancellationToken cancellationToken = default);
}
