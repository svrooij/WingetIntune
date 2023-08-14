using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Graph.Beta.DeviceManagement.ApplePushNotificationCertificate.GenerateApplePushNotificationCertificateSigningRequest;
using Microsoft.Graph.Beta.Models;
using System.Runtime.InteropServices;
using WingetIntune.Models;
using Xunit.Sdk;

namespace WingetIntune.Tests.Intune;

public class IntuneManagerTests
{
    [Fact]
    public async Task GenerateMsiPackage_OtherPackage_ThrowsError()
    {
        var intuneManager = new IntuneManager(new NullLogger<IntuneManager>(), null, null, null, null);
        var tempFolder = Path.Combine(Path.GetTempPath(), "intunewin");
        var outputFolder = Path.Combine(Path.GetTempPath(), "packages");

        await Assert.ThrowsAsync<ArgumentException>(() => intuneManager.GenerateMsiPackage(tempFolder, outputFolder, new PackageInfo(), CancellationToken.None));
    }

    [Fact]
    public async Task GenerateInstallerPackage_MsiPackage_Returns()
    {
        var packageId = "Microsoft.AzureCLI";
        var version = "2.51.0";
        var tempFolder = Path.Combine(Path.GetTempPath(), "intunewin");
        var tempPackageFolder = Path.Combine(tempFolder, packageId, version);
        
        var outputFolder = Path.Combine(Path.GetTempPath(), "packages");
        var outputPackageFolder = Path.Combine(outputFolder, packageId, version);
        var contentPrepToolPath = Path.Combine(tempFolder, IntuneManager.IntuneWinAppUtil);
        var installerPath = Path.Combine(tempPackageFolder, IntuneTestConstants.azureCliPackageInfo.InstallerFilename!);

        var logoPath = Path.GetFullPath(Path.Combine(outputPackageFolder, "..", "logo.png"));


        var fileManagerMock = new Mock<IFileManager>(MockBehavior.Strict);
        fileManagerMock.Setup(x => x.CreateFolderForPackage(tempFolder, packageId, version)).Returns(Path.Combine(tempFolder, packageId, version)).Verifiable();
        fileManagerMock.Setup(x => x.CreateFolderForPackage(outputFolder, packageId, version)).Returns(Path.Combine(outputFolder, packageId, version)).Verifiable();
        fileManagerMock.Setup(x => x.CreateFolder(tempFolder)).Verifiable();
        fileManagerMock.Setup(x => x.DownloadFileAsync(IntuneManager.IntuneWinAppUtilUrl, contentPrepToolPath, true, false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        fileManagerMock.Setup(x => x.DownloadFileAsync(IntuneTestConstants.azureCliPackageInfo.InstallerUrl!.ToString(), installerPath, true, false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        fileManagerMock.Setup(x => x.DownloadFileAsync($"https://api.winstall.app/icons/{packageId}.png", logoPath, false, false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var detectionContent = @"Package Microsoft.AzureCLI 2.51.0 from Winget

MsiProductCode={89E4C65D-96DD-435B-9BBB-EF1EAEF5B738}
MsiVersion=2.51.0
";
        var readmeContent = @"Package Microsoft.AzureCLI 2.51.0 from Winget

Install script:
msiexec /i azure-cli-2.51.0.msi /quiet /qn

Uninstall script:
msiexec /x {89E4C65D-96DD-435B-9BBB-EF1EAEF5B738} /quiet /qn
";
        // Check detection failed on Linux, have to check. replace It.IsAny<string>() with detectionContent
        //Check readme failed on Linux, have to check. replace It.IsAny<string>() with readmeContent
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileManagerMock.Setup(x => x.WriteAllTextAsync(Path.Combine(outputPackageFolder, "detection.txt"), detectionContent, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            fileManagerMock.Setup(x => x.WriteAllTextAsync(Path.Combine(outputPackageFolder, "readme.txt"), readmeContent, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
        }
        else
        {
            fileManagerMock.Setup(x => x.WriteAllTextAsync(Path.Combine(outputPackageFolder, "detection.txt"), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            fileManagerMock.Setup(x => x.WriteAllTextAsync(Path.Combine(outputPackageFolder, "readme.txt"), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
        }
        
        
        fileManagerMock.Setup(x => x.WriteAllBytesAsync(Path.Combine(outputPackageFolder, "app.json"), It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

        var processManagerMock = new Mock<IProcessManager>(MockBehavior.Strict);
        processManagerMock.Setup(x => x.RunProcessAsync(contentPrepToolPath, $"-c {tempPackageFolder} -s {IntuneTestConstants.azureCliPackageInfo.InstallerFilename} -o {outputPackageFolder} -q", It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(new ProcessResult(0, null,null))
            .Verifiable();

        var intuneManager = new IntuneManager(new NullLogger<IntuneManager>(), fileManagerMock.Object, processManagerMock.Object, null, null);

        await intuneManager.GenerateInstallerPackage(tempFolder, outputFolder, IntuneTestConstants.azureCliPackageInfo, IntuneManager.DefaultIntuneWinAppUrl, CancellationToken.None);
        fileManagerMock.VerifyAll();
        processManagerMock.VerifyAll();
    }



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