# WinTuner PowerShell module

[![Documentation badge](https://img.shields.io/badge/Show_Documentation-darkblue?style=for-the-badge)](https://wintuner.app/)
[![PowerShell gallery version][badge_powershell]][link_powershell]
[![PowerShell gallery downloads][badge_powershell_downloads]][link_powershell]
[![License][badge_license]][link_license]

Source of WinTuner PowerShell module, available in the [PowerShell Gallery][link_powershell].

Documentation can be found [here](https://wintuner.app/docs/category/wintuner-powershell).

## Development

To regenerate the documentation, run the following command from the root of this repo:

```powershell
# Install the tool
# dotnet tool install --global SvRooij.PowerShell.Docs --version 0.1.1
PS> pwsh-docs --dll .\src\Svrooij.WinTuner.CmdLets\bin\Debug\net6.0\Svrooij.WinTuner.CmdLets.dll --use-xml-docs --maml-file .\src\Svrooij.WinTuner.CmdLets\Svrooij.WinTuner.CmdLets.dll-Help.xml
```

[badge_license]: https://img.shields.io/github/license/svrooij/WingetIntune?style=for-the-badge
[link_license]: https://github.com/svrooij/WingetIntune/blob/main/LICENSE.txt
[badge_powershell]: https://img.shields.io/powershellgallery/v/WinTuner?style=for-the-badge&logo=powershell&logoColor=white
[badge_powershell_downloads]: https://img.shields.io/powershellgallery/dt/WinTuner?style=for-the-badge&logo=powershell&logoColor=white
[link_powershell]: https://www.powershellgallery.com/packages/WinTuner/
