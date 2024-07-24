using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.InteropServices;
using Winget.CommunityRepository.Models;
using WingetIntune.Interfaces;
using WingetIntune.Models;

namespace WingetIntune.Tests.Intune;

public class IntuneManagerTests
{
    [Fact]
    public async Task GenerateMsiPackage_OtherPackage_ThrowsError()
    {
        var intuneManager = new IntuneManager(null, null, null, null, null, null, null, null, null, null);
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

        var fileManager = Substitute.For<IFileManager>();
        fileManager.CreateFolderForPackage(tempFolder, packageId, version).Returns(Path.Combine(tempFolder, packageId, version));
        fileManager.CreateFolderForPackage(outputFolder, packageId, version).Returns(Path.Combine(outputFolder, packageId, version));
        fileManager.DownloadFileAsync(installer.InstallerUrl!.ToString(), installerPath, null, true, false, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        fileManager.DownloadFileAsync($"https://api.winstall.app/icons/{packageId}.png", logoPath, null, false, false, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var detectionContent = @"Package Microsoft.AzureCLI 2.51.0 from Winget

MsiProductCode={89E4C65D-96DD-435B-9BBB-EF1EAEF5B738}
MsiVersion=2.51.0
";

        var readmeContent = @"Package Microsoft.AzureCLI 2.51.0 from Winget

Display name: Microsoft Azure CLI
Publisher: Microsoft Corporation
Homepage: 

Install script:
msiexec /i azure-cli-2.51.0-x64.msi /quiet /qn

Uninstall script:
msiexec /x {89E4C65D-96DD-435B-9BBB-EF1EAEF5B738} /quiet /qn

Description:
The Azure command-line interface (Azure CLI) is a set of commands used to create and manage Azure resources. The Azure CLI is available across Azure services and is designed to get you working quickly with Azure, with an emphasis on automation.
";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileManager.WriteAllTextAsync(Path.Combine(outputPackageFolder, "detection.txt"), detectionContent, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            fileManager.WriteAllTextAsync(Path.Combine(outputPackageFolder, "readme.txt"), readmeContent, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        }
        else
        {
            fileManager.WriteAllTextAsync(Path.Combine(outputPackageFolder, "detection.txt"), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            fileManager.WriteAllTextAsync(Path.Combine(outputPackageFolder, "readme.txt"), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        }

        fileManager.WriteAllBytesAsync(Path.Combine(outputPackageFolder, "app.json"), Arg.Any<byte[]>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var processManager = Substitute.For<IProcessManager>();

        var intunePackager = Substitute.For<IIntunePackager>();

        var intuneManager = new IntuneManager(new NullLoggerFactory(), fileManager, processManager, null, null, null, intunePackager, null, null, null);

        await intuneManager.GenerateInstallerPackage(tempFolder, outputFolder, IntuneTestConstants.azureCliPackageInfo, new PackageOptions { Architecture = Models.Architecture.X64, InstallerContext = InstallerContext.User }, CancellationToken.None);

        fileManager.Received().CreateFolderForPackage(tempFolder, packageId, version);
        fileManager.Received().CreateFolderForPackage(outputFolder, packageId, version);
        await fileManager.Received().DownloadFileAsync(installer.InstallerUrl!.ToString(), installerPath, null, true, false, Arg.Any<CancellationToken>());
        await fileManager.Received().DownloadFileAsync($"https://api.winstall.app/icons/{packageId}.png", logoPath, null, false, false, Arg.Any<CancellationToken>());
        await fileManager.Received().WriteAllBytesAsync(Path.Combine(outputPackageFolder, "app.json"), Arg.Any<byte[]>(), Arg.Any<CancellationToken>());

    }

    [Fact]
    public async Task DownloadLogoAsync_CallsFilemanager()
    {
        var packageId = "Microsoft.AzureCLI";
        var version = "2.26.1";
        var folder = Path.Combine(Path.GetTempPath(), "intunewin", packageId, version);

        var logoPath = Path.GetFullPath(Path.Combine(folder, "..", "logo.png"));

        var fileManager = Substitute.For<IFileManager>();
        fileManager.DownloadFileAsync($"https://api.winstall.app/icons/{packageId}.png", logoPath, null, false, false, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var intuneManager = new IntuneManager(new NullLoggerFactory(), fileManager, null, null, null, null, null, null, null, null);
        await intuneManager.DownloadLogoAsync(folder, packageId, CancellationToken.None);

        //call.Received(1);

    }

    [Fact]
    public async Task DownloadInstallerAsync_CallsFilemanager()
    {
        var packageId = "Microsoft.AzureCLI";
        var version = "2.26.1";
        var hash = "1234567890";
        var folder = Path.Combine(Path.GetTempPath(), "intunewin", packageId, version);

        var packageInfo = new PackageInfo
        {
            InstallerFilename = "testpackage.exe",
            InstallerUrl = new Uri("https://localhost/testpackage.exe"),
            Installer = new WingetInstaller
            {
                InstallerType = "exe",
                InstallerSha256 = hash
            }
        };

        var installerPath = Path.GetFullPath(Path.Combine(folder, packageInfo.InstallerFilename));

        var fileManager = Substitute.For<IFileManager>();
        fileManager.DownloadFileAsync(packageInfo.InstallerUrl.ToString(), installerPath, hash, true, false, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var intuneManager = new IntuneManager(new NullLoggerFactory(), fileManager, null, null, null, null, null, null, null, null);
        await intuneManager.DownloadInstallerAsync(folder, packageInfo, CancellationToken.None);

        //call.Received(1);
    }
}
