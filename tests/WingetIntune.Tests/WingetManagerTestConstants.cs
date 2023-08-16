namespace WingetIntune.Tests;

internal class WingetManagerTestConstants
{
    internal const string wingetListNoInstalledPackage = @"No installed package found matching input criteria.";

    internal const string wingetListInstalledPackage = @"Name                   Id                  Version Source
----------------------------------------------------------
Notepad++ (64-bit x64) Notepad++.Notepad++ 8.5.4   winget";

    internal const string ohMyPoshOutput = @"Found Oh My Posh [JanDeDobbeleer.OhMyPosh]
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

    internal const string powershellOutput = @"Found PowerShell [9MZ1SNWT0N5D]
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

    internal const string ohMyPoshYaml = @"# Created using wingetcreate 1.5.1.0
# yaml-language-server: $schema=https://aka.ms/winget-manifest.version.1.4.0.schema.json

PackageIdentifier: JanDeDobbeleer.OhMyPosh
PackageVersion: 18.3.3
DefaultLocale: en-US
ManifestType: version
ManifestVersion: 1.4.0";

    internal const string ohMyPoshInstallYaml = @"# Created using wingetcreate 1.5.1.0
# yaml-language-server: $schema=https://aka.ms/winget-manifest.installer.1.4.0.schema.json

PackageIdentifier: JanDeDobbeleer.OhMyPosh
PackageVersion: 18.3.3
Platform:
- Windows.Desktop
MinimumOSVersion: 10.0.0.0
InstallerType: inno
InstallModes:
- interactive
- silent
- silentWithProgress
Installers:
- Architecture: x64
  Scope: machine
  InstallerUrl: https://github.com/JanDeDobbeleer/oh-my-posh/releases/download/v18.3.3/install-amd64.exe
  InstallerSha256: D2732860CFEDAD53F7B06962D27F6EFA9C4F529086EC02D16808C378974AE20A
  InstallerSwitches:
    Custom: /INSTALLER=winget /ALLUSERS
- Architecture: x64
  Scope: user
  InstallerUrl: https://github.com/JanDeDobbeleer/oh-my-posh/releases/download/v18.3.3/install-amd64.exe
  InstallerSha256: D2732860CFEDAD53F7B06962D27F6EFA9C4F529086EC02D16808C378974AE20A
  InstallerSwitches:
    Custom: /INSTALLER=winget /CURRENTUSER
- Architecture: x86
  Scope: machine
  InstallerUrl: https://github.com/JanDeDobbeleer/oh-my-posh/releases/download/v18.3.3/install-386.exe
  InstallerSha256: 974C596F7E97BEB7E3FFF3D37DE70CB24CCB3807303848678B04F0240620C8B8
  InstallerSwitches:
    Custom: /INSTALLER=winget /ALLUSERS
- Architecture: x86
  Scope: user
  InstallerUrl: https://github.com/JanDeDobbeleer/oh-my-posh/releases/download/v18.3.3/install-386.exe
  InstallerSha256: 974C596F7E97BEB7E3FFF3D37DE70CB24CCB3807303848678B04F0240620C8B8
  InstallerSwitches:
    Custom: /INSTALLER=winget /CURRENTUSER
- Architecture: arm64
  Scope: machine
  InstallerUrl: https://github.com/JanDeDobbeleer/oh-my-posh/releases/download/v18.3.3/install-arm64.exe
  InstallerSha256: D9F7A215233F7F2C6317EC5864BB70CE587CA4BAA39B78C124FCE6FCE5408F2F
  InstallerSwitches:
    Custom: /INSTALLER=winget /ALLUSERS
- Architecture: arm64
  Scope: user
  InstallerUrl: https://github.com/JanDeDobbeleer/oh-my-posh/releases/download/v18.3.3/install-arm64.exe
  InstallerSha256: D9F7A215233F7F2C6317EC5864BB70CE587CA4BAA39B78C124FCE6FCE5408F2F
  InstallerSwitches:
    Custom: /INSTALLER=winget /CURRENTUSER
ManifestType: installer
ManifestVersion: 1.4.0
ReleaseDate: 2023-08-09";

    internal const string ohMyPoshLocaleYaml = @"# Created using wingetcreate 1.5.1.0
# yaml-language-server: $schema=https://aka.ms/winget-manifest.defaultLocale.1.4.0.schema.json

PackageIdentifier: JanDeDobbeleer.OhMyPosh
PackageVersion: 18.3.3
PackageLocale: en-US
Publisher: Jan De Dobbeleer
PublisherUrl: https://github.com/JanDeDobbeleer/oh-my-posh/
PublisherSupportUrl: https://github.com/JanDeDobbeleer/oh-my-posh/issues
Author: Jan De Dobbeleer
PackageName: Oh My Posh
PackageUrl: https://ohmyposh.dev/
License: MIT
LicenseUrl: https://github.com/JanDeDobbeleer/oh-my-posh/raw/main/COPYING
ShortDescription: Prompt theme engine for any shell
Moniker: oh-my-posh
Tags:
- console
- command-line
- shell
- command-prompt
- powershell
- wsl
- developer-tools
- utilities
- cli
- cmd
- ps
- terminal
- oh-my-posh
ReleaseNotesUrl: https://github.com/JanDeDobbeleer/oh-my-posh/releases/tag/v18.3.3
ManifestType: defaultLocale
ManifestVersion: 1.4.0";
}