using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WingetIntune.Models;

namespace WingetIntune.Tests
{
    public class WingetManagerTests
    {
        private readonly ILogger<WingetManager> logger;

        public WingetManagerTests()
        {
            logger = new NullLogger<WingetManager>();
        }

        [Theory]
        [InlineData(WingetManagerTestConstants.wingetListNoInstalledPackage, IsInstalledResult.NotInstalled)]
        [InlineData(WingetManagerTestConstants.wingetListInstalledPackage, IsInstalledResult.Installed)]
        public async Task CheckInstalled_NoVersion_ReturnsCorrectValue(string mockedResponse, IsInstalledResult expectedResult)
        {
            var packageId = "Notepad++.Notepad++";
            var processManager = new Mock<IProcessManager>(MockBehavior.Strict);
            processManager.Setup(x => x.RunProcessAsync("winget",
                               $"list --id {packageId} --exact --disable-interactivity --accept-source-agreements",
                                              It.IsAny<CancellationToken>(), false))
                .ReturnsAsync(new ProcessResult(0, mockedResponse, string.Empty));
            var wingetManager = new WingetManager(logger, processManager.Object, null);
            var result = await wingetManager.CheckInstalled(packageId, null);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("8.5.4", WingetManagerTestConstants.wingetListNoInstalledPackage, IsInstalledResult.NotInstalled)]
        [InlineData("8.5.4", WingetManagerTestConstants.wingetListInstalledPackage, IsInstalledResult.Installed)]
        [InlineData("8.5.9", WingetManagerTestConstants.wingetListInstalledPackage, IsInstalledResult.UpgradeAvailable)]
        public async Task CheckInstalled_WithVersion_ReturnsCorrectValue(string version, string mockedResponse, IsInstalledResult expectedResult)
        {
            var packageId = "Notepad++.Notepad++";
            var processManager = new Mock<IProcessManager>(MockBehavior.Strict);
            processManager.Setup(x => x.RunProcessAsync("winget",
                               $"list --id {packageId} --exact --disable-interactivity --accept-source-agreements",
                                              It.IsAny<CancellationToken>(), false))
                .ReturnsAsync(new ProcessResult(0, mockedResponse, string.Empty));
            var wingetManager = new WingetManager(logger, processManager.Object, null);
            var result = await wingetManager.CheckInstalled(packageId, version);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetPackageInfoAsync_ParsesResponse_WingetResult()
        {
            var packageId = "JanDeDobbeleer.OhMyPosh";
            var version = "18.3.1";
            var processManager = new Mock<IProcessManager>(MockBehavior.Strict);
            processManager.Setup(x => x.RunProcessAsync("winget",
                $"show --id {packageId} --version {version} --exact --accept-source-agreements --disable-interactivity",
                It.IsAny<CancellationToken>(),
                false))
                .ReturnsAsync(new ProcessResult(0, WingetManagerTestConstants.ohMyPoshOutput, string.Empty))
                .Verifiable();
            var wingetManager = new WingetManager(logger, processManager.Object, null);
            var info = await wingetManager.GetPackageInfoAsync(packageId, version, null);

            processManager.VerifyAll();

            // Check PackageId, Name, Version, Publisher, PublisherUrl, HomePageUrl, InstallerType, InstallerUrl, Hash
            Assert.Equal("JanDeDobbeleer.OhMyPosh", info.PackageIdentifier);
            Assert.Equal("Oh My Posh", info.DisplayName);
            Assert.Equal("18.3.1", info.Version);
            Assert.Equal("Jan De Dobbeleer", info.Publisher);
            Assert.Equal("Prompt theme engine for any shell", info.Description);
            Assert.Equal("https://github.com/JanDeDobbeleer/oh-my-posh/", info.PublisherUrl!.ToString());
            Assert.Equal("https://github.com/JanDeDobbeleer/oh-my-posh/issues", info.SupportUrl!.ToString());
            Assert.Equal("https://ohmyposh.dev/", info.InformationUrl!.ToString());
            Assert.Equal(InstallerType.Inno, info.InstallerType);
            Assert.Equal("https://github.com/JanDeDobbeleer/oh-my-posh/releases/download/v18.3.1/install-amd64.exe", info.InstallerUrl!.ToString());
            Assert.Equal("fc587e29525d2a9db7a46a98997b351ba1c2b699167f6ad8e22a23e261d526e9", info.Hash);
        }

        [Fact]
        public async Task GetPackageInfoAsync_DownloadsData_WingetResult()
        {
            var packageId = "JanDeDobbeleer.OhMyPosh";
            var version = "18.3.3";
            var source = "winget";
            var processManager = new Mock<IProcessManager>(MockBehavior.Strict);
            var filemanagerMock = new Mock<IFileManager>(MockBehavior.Strict);
            filemanagerMock.Setup(x => x.DownloadStringAsync(WingetManager.CreateManifestUri(packageId, version, null), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(WingetManagerTestConstants.ohMyPoshYaml)
                .Verifiable();
            filemanagerMock.Setup(x => x.DownloadStringAsync(WingetManager.CreateManifestUri(packageId, version, ".installer"), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(WingetManagerTestConstants.ohMyPoshInstallYaml)
                .Verifiable();
            filemanagerMock.Setup(x => x.DownloadStringAsync(WingetManager.CreateManifestUri(packageId, version, ".locale.en-US"), true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(WingetManagerTestConstants.ohMyPoshLocaleYaml)
                .Verifiable();
            var wingetManager = new WingetManager(logger, processManager.Object, filemanagerMock.Object);
            var info = await wingetManager.GetPackageInfoAsync(packageId, version, source);

            filemanagerMock.VerifyAll();

            // Check PackageId, Name, Version, Publisher, PublisherUrl, HomePageUrl, InstallerType, InstallerUrl, Hash
            Assert.Equal("JanDeDobbeleer.OhMyPosh", info.PackageIdentifier);
            Assert.Equal("Oh My Posh", info.DisplayName);
            Assert.Equal("18.3.3", info.Version);
            Assert.Equal("Jan De Dobbeleer", info.Publisher);
            Assert.Equal("Prompt theme engine for any shell", info.Description);
            Assert.Equal("https://github.com/JanDeDobbeleer/oh-my-posh/", info.PublisherUrl!.ToString());
            Assert.Equal("https://github.com/JanDeDobbeleer/oh-my-posh/issues", info.SupportUrl!.ToString());
            Assert.Equal("https://ohmyposh.dev/", info.InformationUrl!.ToString());
            Assert.Equal(InstallerType.Inno, info.InstallerType);
            Assert.NotEmpty(info.Installers!);
        }

        [Fact]
        public async Task GetPackageInfoAsync_ParsesResponse_StoreResult()
        {
            var packageId = "9MZ1SNWT0N5D";
            var processManager = new Mock<IProcessManager>(MockBehavior.Strict);
            processManager.Setup(x => x.RunProcessAsync("winget",
                $"show --id {packageId} --exact --accept-source-agreements --disable-interactivity",
                It.IsAny<CancellationToken>(),
                false))
                .ReturnsAsync(new ProcessResult(0, WingetManagerTestConstants.powershellOutput, string.Empty))
                .Verifiable();
            var wingetManager = new WingetManager(logger, processManager.Object, null);
            var info = await wingetManager.GetPackageInfoAsync(packageId, null, null);

            processManager.VerifyAll();

            // Check PackageId, Name, Version, Publisher, PublisherUrl, HomePageUrl, InstallerType, InstallerUrl, Hash
            Assert.Equal("9MZ1SNWT0N5D", info.PackageIdentifier);
            Assert.Equal("PowerShell", info.DisplayName);
            Assert.Equal("Unknown", info.Version);
            Assert.Equal("Microsoft Corporation", info.Publisher);
            Assert.Equal("https://github.com/powershell/powershell", info.PublisherUrl!.ToString());
            Assert.Equal("https://github.com/PowerShell/PowerShell/issues", info.SupportUrl!.ToString());
            var containsSecondLine = info.Description!.Contains("PowerShell commands let you manage computers from the command line.");
            Assert.True(containsSecondLine, "Contains second line");
        }

        [Fact]
        public async Task GetPackageInfoAsync_ThrowsException()
        {
            var packageId = "JanDeDobbeleer.OhMyPosh";
            var processManager = new Mock<IProcessManager>(MockBehavior.Strict);
            processManager.Setup(x => x.RunProcessAsync("winget",
                $"show --id {packageId} --exact --accept-source-agreements --disable-interactivity",
                It.IsAny<CancellationToken>(),
                false))
                .ReturnsAsync(new ProcessResult(10, string.Empty, "Something went terribly wrong"))
                .Verifiable();
            var wingetManager = new WingetManager(logger, processManager.Object, null);
            await Assert.ThrowsAsync<Exception>(() => wingetManager.GetPackageInfoAsync(packageId, null, null));

            processManager.VerifyAll();
        }

        [Theory]
        [InlineData(null, null, false)]
        [InlineData(null, null, true)]
        [InlineData("0.1.1", null, false)]
        [InlineData("0.1.1", null, true)]
        [InlineData("0.1.1", "winget", true)]
        [InlineData("0.1.1", "store", false)]
        [InlineData(null, "winget", true)]
        public async Task Install_CallsCorrectProcess(string? version, string? source, bool force)
        {
            var packageId = "Notepad++.Notepad++";
            var arguments = $"install --id {packageId}";
            if (!string.IsNullOrWhiteSpace(version))
            {
                arguments += $" --version {version}";
            }
            if (!string.IsNullOrWhiteSpace(source))
            {
                arguments += $" --source {source}";
            }
            if (force)
            {
                arguments += " --force";
            }
            arguments += " --silent --accept-package-agreements --accept-source-agreements --disable-interactivity";
            var processManager = new Mock<IProcessManager>(MockBehavior.Strict);
            processManager.Setup(x => x.RunProcessAsync("winget",
                arguments,
                It.IsAny<CancellationToken>(),
                true))
                .ReturnsAsync(new ProcessResult(0, WingetManagerTestConstants.powershellOutput, string.Empty))
                .Verifiable();
            var wingetManager = new WingetManager(logger, processManager.Object, null);

            await wingetManager.Install(packageId, version, source, force);

            processManager.Verify();
        }

        [Theory]
        [InlineData(null, null, false)]
        [InlineData(null, null, true)]
        [InlineData("0.1.1", null, false)]
        [InlineData("0.1.1", null, true)]
        [InlineData("0.1.1", "winget", true)]
        [InlineData("0.1.1", "store", false)]
        [InlineData(null, "winget", true)]
        public async Task Upgrade_CallsCorrectProcess(string? version, string? source, bool force)
        {
            var packageId = "Notepad++.Notepad++";
            var arguments = $"upgrade --id {packageId}";
            if (!string.IsNullOrWhiteSpace(version))
            {
                arguments += $" --version {version}";
            }
            if (!string.IsNullOrWhiteSpace(source))
            {
                arguments += $" --source {source}";
            }
            if (force)
            {
                arguments += " --force";
            }
            arguments += " --silent --accept-package-agreements --accept-source-agreements --disable-interactivity";
            var processManager = new Mock<IProcessManager>(MockBehavior.Strict);
            processManager.Setup(x => x.RunProcessAsync("winget",
                arguments,
                It.IsAny<CancellationToken>(),
                true))
                .ReturnsAsync(new ProcessResult(0, WingetManagerTestConstants.powershellOutput, string.Empty))
                .Verifiable();
            var wingetManager = new WingetManager(logger, processManager.Object, null);

            await wingetManager.Upgrade(packageId, version, source, force);

            processManager.Verify();
        }
    }
}
