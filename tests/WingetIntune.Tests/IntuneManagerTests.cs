using Microsoft.Extensions.Logging.Abstractions;
using WingetIntune.Models;

namespace WingetIntune.Tests;

public class IntuneManagerTests
{
    [Fact]
    public async Task DownloadLogoAsync_CallsFilemanager()
    {
        var packageId = "Microsoft.AzureCLI";
        var version = "2.26.1";
        var folder = Path.Combine(Path.GetTempPath(), "intunewin", packageId, version);

        var logoPath = Path.GetFullPath(Path.Combine(folder, "..", "logo.png"));

        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(x => x.DownloadFileAsync($"https://api.winstall.app/icons/{packageId}.png", logoPath, false, false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var intuneManager = new IntuneManager(new NullLogger<IntuneManager>(), fileManagerMock.Object, null, null, null);
        await intuneManager.DownloadLogoAsync(folder, packageId, CancellationToken.None);

        fileManagerMock.Verify();
    }

    [Fact]
    public async Task DownloadInstallerAsync_CallsFilemanager()
    {
        var packageId = "Microsoft.AzureCLI";
        var version = "2.26.1";
        var folder = Path.Combine(Path.GetTempPath(), "intunewin", packageId, version);

        var packageInfo = new PackageInfo
        {
            InstallerFilename = "testpackage.exe",
            InstallerUrl = new Uri("https://localhost/testpackage.exe")
        };

        var installerPath = Path.GetFullPath(Path.Combine(folder, packageInfo.InstallerFilename));

        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(x => x.DownloadFileAsync(packageInfo.InstallerUrl.ToString(), installerPath, true, false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var intuneManager = new IntuneManager(new NullLogger<IntuneManager>(), fileManagerMock.Object, null, null, null);
        await intuneManager.DownloadInstallerAsync(folder, packageInfo, CancellationToken.None);

        fileManagerMock.Verify();
    }

    [Fact]
    public async Task DownloadContentPrepToolAsync_CallsFilemanager()
    {
        var tempFolder = Path.GetTempPath();
        var contentPrepToolPath = Path.Combine(tempFolder, IntuneManager.IntuneWinAppUtil);

        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(x => x.CreateFolder(tempFolder));
        fileManagerMock.Setup(x => x.DownloadFileAsync(IntuneManager.IntuneWinAppUtilUrl, contentPrepToolPath, true, false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var intuneManager = new IntuneManager(new NullLogger<IntuneManager>(), fileManagerMock.Object, null, null, null);
        await intuneManager.DownloadContentPrepToolAsync(tempFolder, IntuneManager.DefaultIntuneWinAppUrl, CancellationToken.None);

        fileManagerMock.Verify();
    }
}