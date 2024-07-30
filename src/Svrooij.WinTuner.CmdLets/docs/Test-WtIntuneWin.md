---
external help file: Svrooij.WinTuner.CmdLets.dll-Help.xml
Module Name: Svrooij.WinTuner.CmdLets
online version: https://wintuner.app/docs/wintuner-powershell/Test-WtIntuneWin
schema: 2.0.0
---

# Test-WtIntuneWin

## SYNOPSIS
Test if a package will install

## SYNTAX

### PackageFolder (Default)
```
Test-WtIntuneWin [-PackageFolder] <String> [[-InstallerArguments] <String>] [-Clean] [-Sleep <Int32>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### WinGet
```
Test-WtIntuneWin [-PackageId] <String> [-Version] <String> [-RootPackageFolder] <String> [-Clean]
 [-Sleep <Int32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### IntuneWin
```
Test-WtIntuneWin [-IntuneWinFile] <String> [[-InstallerFilename] <String>] [[-InstallerArguments] <String>]
 [-Clean] [-Sleep <Int32>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Test if a package will install on the Windows Sandbox

## EXAMPLES

### Example 1
```powershell
PS C:\> Test-WtIntuneWin -PackageFolder D:\packages\JanDeDobbeleer.OhMyPosh\22.0.3
```

Test a packaged installer in sandbox

## PARAMETERS

### -Clean
Clean the test files after run

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InstallerArguments
The installer arguments (if you want it to execute silently)

```yaml
Type: String
Parameter Sets: PackageFolder
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: IntuneWin
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InstallerFilename
The installer filename (if not set correctly inside the intunewin)

```yaml
Type: String
Parameter Sets: IntuneWin
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IntuneWinFile
The IntuneWin file to test

```yaml
Type: String
Parameter Sets: IntuneWin
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
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

### -Sleep
Sleep for x seconds before auto shutdown

```yaml
Type: Int32
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

### System.String

## OUTPUTS

### System.String

## NOTES

## RELATED LINKS
