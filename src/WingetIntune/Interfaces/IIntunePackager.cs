namespace WingetIntune.Interfaces;

public interface IIntunePackager
{
    Task CreatePackage(string inputFolder, string outputFolder, string installerFilename, CancellationToken cancellationToken);
}
