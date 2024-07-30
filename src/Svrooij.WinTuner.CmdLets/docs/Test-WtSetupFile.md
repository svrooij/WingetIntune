---
external help file: Svrooij.WinTuner.CmdLets.dll-Help.xml
Module Name: Svrooij.WinTuner.CmdLets
online version: https://wintuner.app/docs/wintuner-powershell/Test-WtSetupFile
schema: 2.0.0
---

# Test-WtSetupFile

## SYNOPSIS
Test your silent install switches

## SYNTAX

```
Test-WtSetupFile [-SetupFile] <String> [[-InstallerArguments] <String>] [-Sleep <Int32>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Test if a setup will install on the Windows Sandbox

## EXAMPLES

### Example 1
```powershell
PS C:\> Test-WtSetupFile -SetupFile D:\packages\xyz.exe -Installer "all your arguments"
```

Test any installer in sandbox

## PARAMETERS

### -InstallerArguments
Override the installer arguments

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SetupFile
Absolute path to your setup file

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
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
