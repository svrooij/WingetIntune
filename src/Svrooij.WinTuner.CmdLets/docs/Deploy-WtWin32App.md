---
external help file: Svrooij.WinTuner.CmdLets.dll-Help.xml
Module Name: Svrooij.WinTuner.CmdLets
online version: https://wintuner.app/docs/wintuner-powershell/Deploy-WtWin32App
schema: 2.0.0
---

# Deploy-WtWin32App

## SYNOPSIS
Create a Win32Lob app in Intune

## SYNTAX

### Win32LobApp (Default)
```
Deploy-WtWin32App [-App] <Win32LobApp> [-IntuneWinFile] <String> [[-LogoPath] <String>]
 [-OverrideAppName <String>] [-GraphId <String>] [-Categories <String[]>] [-AvailableFor <String[]>]
 [-RequiredFor <String[]>] [-UninstallFor <String[]>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### WinGet
```
Deploy-WtWin32App [-PackageId] <String> [-Version] <String> [-RootPackageFolder] <String>
 [-OverrideAppName <String>] [-GraphId <String>] [-Categories <String[]>] [-AvailableFor <String[]>]
 [-RequiredFor <String[]>] [-UninstallFor <String[]>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### PackageFolder
```
Deploy-WtWin32App [-PackageFolder] <String> [-OverrideAppName <String>] [-GraphId <String>]
 [-Categories <String[]>] [-AvailableFor <String[]>] [-RequiredFor <String[]>] [-UninstallFor <String[]>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Use this command to upload an intunewin package to Microsoft Intune as a new Win32LobApp.

## EXAMPLES

### Example 1
```powershell
PS C:\> Deploy-WtWin32App -PackageFolder C:\Tools\packages\JanDeDobbeleer.OhMyPosh\19.5.2
```

Upload a pre-packaged application, from just it's folder

## PARAMETERS

### -App
The App configuration you want to create

```yaml
Type: Win32LobApp
Parameter Sets: Win32LobApp
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -AvailableFor
Groups that the app should available for, Group Object ID or 'AllUsers'/'AllDevices'

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Categories
Categories to add to the app

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -GraphId
Graph ID of the app to supersede

```yaml
Type: String
Parameter Sets: (All)
Aliases: AppId

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IntuneWinFile
The .intunewin file that should be added to this app

```yaml
Type: String
Parameter Sets: Win32LobApp
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -LogoPath
Load the logo from file

```yaml
Type: String
Parameter Sets: Win32LobApp
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -OverrideAppName
Override the name of the app in Intune

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PackageFolder
The folder where the package is

```yaml
Type: String
Parameter Sets: PackageFolder
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -PackageId
The package id to upload to Intune.

```yaml
Type: String
Parameter Sets: WinGet
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RequiredFor
Groups that the app is required for, Group Object ID or 'AllUsers'/'AllDevices'

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RootPackageFolder
The Root folder where all the package live in.

```yaml
Type: String
Parameter Sets: WinGet
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UninstallFor
Groups that the app should be uninstalled for, Group Object ID or 'AllUsers'/'AllDevices'

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Version
The version to upload to Intune

```yaml
Type: String
Parameter Sets: WinGet
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### Microsoft.Graph.Beta.Models.Win32LobApp

### System.String

## OUTPUTS

### Microsoft.Graph.Beta.Models.Win32LobApp

## NOTES

## RELATED LINKS

[https://wintuner.app/docs/wintuner-powershell/Deploy-WtWin32App](https://wintuner.app/docs/wintuner-powershell/Deploy-WtWin32App)

