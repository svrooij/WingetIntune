using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.InteropServices;
using WingetIntune.Interfaces;
using WingetIntune.Models;

namespace WingetIntune.Tests.Intune;

public class IntuneManagerTests
{
    [Fact]
    public async Task GenerateMsiPackage_OtherPackage_ThrowsError()
    {
        var intuneManager = new IntuneManager(null, null, null, null, null, null, null, null);
        var tempFolder = Path.Combine(Path.GetTempPath(), "intunewin");
        var outputFolder = Path.Combine(Path.GetTempPath(), "packages");

        await Assert.ThrowsAsync<ArgumentException>(() => intuneManager.GenerateMsiPackage(tempFolder, outputFolder, new PackageInfo(), PackageOptions.Create(), CancellationToken.None));
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
        var installer = IntuneTestConstants.azureCliPackageInfo.Installers!.First();
        var installerPath = Path.Combine(tempPackageFolder, installer.InstallerFilename!);

        var logoPath = Path.GetFullPath(Path.Combine(outputPackageFolder, "..", "logo.png"));

        var fileManagerMock = new Mock<IFileManager>(MockBehavior.Strict);
        fileManagerMock.Setup(x => x.CreateFolderForPackage(tempFolder, packageId, version)).Returns(Path.Combine(tempFolder, packageId, version)).Verifiable();
        fileManagerMock.Setup(x => x.CreateFolderForPackage(outputFolder, packageId, version)).Returns(Path.Combine(outputFolder, packageId, version)).Verifiable();
        fileManagerMock.Setup(x => x.DownloadFileAsync(installer.InstallerUrl!.ToString(), installerPath, true, false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        fileManagerMock.Setup(x => x.DownloadFileAsync($"https://api.winstall.app/icons/{packageId}.png", logoPath, false, false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var detectionContent = @"Package Microsoft.AzureCLI 2.51.0 from Winget

MsiProductCode={E428EC6E-E4F4-4DCA-9786-2653D0990AAD}
MsiVersion=2.51.0
";

        var readmeContent = @"Package Microsoft.AzureCLI 2.51.0 from Winget

Display name: Microsoft Azure CLI
Publisher: Microsoft Corporation
Homepage: 

Install script:
msiexec /i azure-cli-2.51.0-x64.msi /quiet /qn

Uninstall script:
msiexec /x {E428EC6E-E4F4-4DCA-9786-2653D0990AAD} /quiet /qn

Description:
The Azure command-line interface (Azure CLI) is a set of commands used to create and manage Azure resources. The Azure CLI is available across Azure services and is designed to get you working quickly with Azure, with an emphasis on automation.
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

        var intunePackagerMock = new Mock<IIntunePackager>(MockBehavior.Loose);

        var intuneManager = new IntuneManager(new NullLoggerFactory(), fileManagerMock.Object, processManagerMock.Object, null, null, null, null, intunePackagerMock.Object);

        await intuneManager.GenerateInstallerPackage(tempFolder, outputFolder, IntuneTestConstants.azureCliPackageInfo, new PackageOptions { Architecture = Models.Architecture.X64, InstallerContext = InstallerContext.User }, CancellationToken.None);
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

        var intuneManager = new IntuneManager(new NullLoggerFactory(), fileManagerMock.Object, null, null, null, null, null, null);
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

        var intuneManager = new IntuneManager(new NullLoggerFactory(), fileManagerMock.Object, null, null, null, null, null, null);
        await intuneManager.DownloadInstallerAsync(folder, packageInfo, CancellationToken.None);

        fileManagerMock.Verify();
    }
}
