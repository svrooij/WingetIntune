using WingetIntune.Models;
namespace WingetIntune.Tests
{
    public class PackageInfoTests
    {
        const string ohMyPoshOutput = @"Found Oh My Posh [JanDeDobbeleer.OhMyPosh]
Version: 18.3.1
Publisher: Jan De Dobbeleer
Publisher Url: https://github.com/JanDeDobbeleer/oh-my-posh/
Publisher Support Url: https://github.com/JanDeDobbeleer/oh-my-posh/issues
Author: Jan De Dobbeleer
Moniker: oh-my-posh
Description: Prompt theme engine for any shell
Homepage: https://ohmyposh.dev/
License: MIT
License Url: https://github.com/JanDeDobbeleer/oh-my-posh/raw/main/COPYING
Release Notes Url: https://github.com/JanDeDobbeleer/oh-my-posh/releases/tag/v18.3.1
Tags:
  console
  command-line
  shell
  command-prompt
  powershell
  wsl
  developer-tools
  utilities
  cli
  cmd
  ps
  terminal
  oh-my-posh
Installer:
  Installer Type: inno
  Installer Url: https://github.com/JanDeDobbeleer/oh-my-posh/releases/download/v18.3.1/install-amd64.exe
  Installer SHA256: fc587e29525d2a9db7a46a98997b351ba1c2b699167f6ad8e22a23e261d526e9
  Release Date: 2023-08-06";

        [Fact]
        public void ParsesOhMyPosh()
        {
            var info = PackageInfo.Parse(ohMyPoshOutput);
            // Check PackageId, Name, Version, Publisher, PublisherUrl, HomePageUrl, InstallerType, InstallerUrl, Hash
            Assert.Equal("JanDeDobbeleer.OhMyPosh", info.PackageId);
            Assert.Equal("Oh My Posh", info.Name);
            Assert.Equal("18.3.1", info.Version);
            Assert.Equal("Jan De Dobbeleer", info.Publisher);
            Assert.Equal("Prompt theme engine for any shell", info.Description);
            Assert.Equal("https://github.com/JanDeDobbeleer/oh-my-posh/", info.PublisherUrl!.ToString());
            Assert.Equal("https://github.com/JanDeDobbeleer/oh-my-posh/issues", info.SupportUrl!.ToString());
            Assert.Equal("https://ohmyposh.dev/", info.HomePageUrl!.ToString());
            Assert.Equal(InstallerType.InnoSetup, info.InstallerType);
            Assert.Equal("https://github.com/JanDeDobbeleer/oh-my-posh/releases/download/v18.3.1/install-amd64.exe", info.InstallerUrl!.ToString());
            Assert.Equal("fc587e29525d2a9db7a46a98997b351ba1c2b699167f6ad8e22a23e261d526e9", info.Hash);
        }

        const string powershellOutput = @"Found PowerShell [9MZ1SNWT0N5D]
Version: Unknown
Publisher: Microsoft Corporation
Publisher Url: https://github.com/powershell/powershell
Publisher Support Url: https://github.com/PowerShell/PowerShell/issues
Description:
  PowerShell is a task-based command-line shell and scripting language built on .NET.  PowerShell helps system administrators and power-users rapidly automate task that manage operating systems (Linux, macOS, and Windows) and processes.

  PowerShell commands let you manage computers from the command line.  PowerShell providers let you access data stores, such as the registry and certificate store, as easily as you access the file system.  PowerShell includes a rich expression parser and a fully developed scripting language.

  PowerShell is Open Source.  See https://github.com/powershell/powershell
License: ms-windows-store://pdp/?ProductId=9MZ1SNWT0N5D
Privacy Url: https://github.com/PowerShell/PowerShell#telemetry
Copyright: Microsoft Corporation
Agreements:
Category: Developer tools
Pricing: Free
Free Trial: No
Terms of Transaction: https://aka.ms/microsoft-store-terms-of-transaction
Seizure Warning: https://aka.ms/microsoft-store-seizure-warning
Store License Terms: https://aka.ms/microsoft-store-license

Installer:
  Installer Type: msstore
  Store Product Id: 9MZ1SNWT0N5D";

        [Fact]
        public void ParsesPowerShell()
        {
            var info = PackageInfo.Parse(powershellOutput);
            // Check PackageId, Name, Version, Publisher, PublisherUrl, HomePageUrl, InstallerType, InstallerUrl, Hash
            Assert.Equal("9MZ1SNWT0N5D", info.PackageId);
            Assert.Equal("PowerShell", info.Name);
            Assert.Equal("Unknown", info.Version);
            Assert.Equal("Microsoft Corporation", info.Publisher);
            Assert.Equal("https://github.com/powershell/powershell", info.PublisherUrl!.ToString());
            Assert.Equal("https://github.com/PowerShell/PowerShell/issues", info.SupportUrl!.ToString());
            var containsSecondLine = info.Description!.Contains("PowerShell commands let you manage computers from the command line.");
            Assert.True(containsSecondLine, "Contains second line");
        }
    }
}