using WingetIntune.Models;

namespace WingetIntune.Tests
{
    public class PackageInfoTests
    {
        [Theory]
        [InlineData(WingetManagerTestConstants.ohMyPoshOutput)]
        [InlineData(WingetManagerTestConstants.ohMyPoshOutputFrench)]
        public void ParsesOhMyPosh(string wingetData)
        {
            var info = PackageInfo.Parse(wingetData);
            // Check PackageId, Name, Version, Publisher, PublisherUrl, HomePageUrl, InstallerType, InstallerUrl, Hash
            Assert.Equal("JanDeDobbeleer.OhMyPosh", info.PackageIdentifier);
            Assert.Equal("Oh My Posh", info.DisplayName);
            Assert.Equal("18.7.0", info.Version);
            Assert.Equal("Jan De Dobbeleer", info.Publisher);
            Assert.Equal("Prompt theme engine for any shell", info.Description);
            Assert.Equal("https://github.com/JanDeDobbeleer/oh-my-posh/", info.PublisherUrl!.ToString());
            Assert.Equal("https://github.com/JanDeDobbeleer/oh-my-posh/issues", info.SupportUrl!.ToString());
            Assert.Equal("https://ohmyposh.dev/", info.InformationUrl!.ToString());
            Assert.Equal(InstallerType.Inno, info.InstallerType);
            Assert.Equal("https://github.com/JanDeDobbeleer/oh-my-posh/releases/download/v18.7.0/install-amd64.exe", info.InstallerUrl!.ToString());
            Assert.Equal("071ceebaafbfbce77352ab2752301aa51938f2601d112574bedbf58773dbda25", info.Hash);
        }

        [Fact]
        public void ParsesPowerShell()
        {
            var info = PackageInfo.Parse(WingetManagerTestConstants.powershellOutput);
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
    }
}
