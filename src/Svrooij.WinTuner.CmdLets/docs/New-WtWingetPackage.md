---
external help file: Svrooij.WinTuner.CmdLets.dll-Help.xml
Module Name: Svrooij.WinTuner.CmdLets
online version: https://wintuner.app/docs/wintuner-powershell/New-WtWingetPackage
schema: 2.0.0
---

# New-WtWingetPackage

## SYNOPSIS
Create intunewin file from Winget installer

## SYNTAX

```
New-WtWingetPackage [-PackageId] <String> [-PackageFolder] <String> [[-Version] <String>]
 [[-TempFolder] <String>] [-Architecture <Architecture>] [-InstallerContext <InstallerContext>]
 [-PackageScript <Boolean>] [-Locale <String>] [-InstallerArguments <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Downloads the installer for the package and creates an \`.intunewin\` file for uploading in Intune.

## EXAMPLES

### Example 1
```
PS C:\> New-WtWingetPackage -PackageId JanDeDobbeleer.OhMyPosh -PackageFolder C:\Tools\Packages
```

Package all files in C:\Temp\Source, with setup file ..\setup.exe to the specified folder

## PARAMETERS

### -PackageFolder
The folder to store the package in

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -PackageId
The package id to download

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -TempFolder
The folder to store temporary files in

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -Version
The version to download (optional)

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
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

### -Architecture
Pick this architecture

```yaml
Type: Architecture
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -InstallerContext
The installer context

```yaml
Type: InstallerContext
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -PackageScript
Package WinGet script, instead of the actual installer. Helpful for installers that don't really work with WinTuner.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -Locale
The desired locale, if available (eg. 'en-US')

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -InstallerArguments
Override the installer arguments

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
## OUTPUTS

### WingetIntune.Models.WingetPackage
## NOTES

## RELATED LINKS
