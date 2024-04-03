# WinTuner PowerShell module

## Refresh documentation

```PowerShell
New-MarkdownHelp -Module "Svrooij.WinTuner.CmdLets" -OutputFolder "..\..\..\docs" -WithModulePage -Force
```

## Create a package and deploy to Intune

```PowerShell
New-WtWingetPackage -PackageId Jandedobbeleer.ohmyposh -PackageFolder C:\tools\packages\ | Deploy-WtWin32App -Username admin@codingstephan.onmicrosoft.com
```