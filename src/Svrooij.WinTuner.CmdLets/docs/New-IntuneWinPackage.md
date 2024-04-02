---
external help file: Svrooij.WinTuner.CmdLets.dll-Help.xml
Module Name: Svrooij.WinTuner.CmdLets
online version: https://wintuner.app/docs/related/content-prep-tool#new-intunewinpackage
schema: 2.0.0
---

# New-IntuneWinPackage

## SYNOPSIS
Create a new IntuneWin package

## SYNTAX

```
New-IntuneWinPackage [-SourcePath] <String> [-SetupFile] <String> [-DestinationPath] <String>
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
This is a re-implementation of the IntuneWinAppUtil.exe tool, it's not feature complete use at your own risk.

## EXAMPLES

### Example 1
```
PS C:\> New-IntuneWinPackage -SourcePath C:\Temp\Source -SetupFile C:\Temp\Source\setup.exe -DestinationPath C:\Temp\Destination
```

Package all files in C:\Temp\Source, with setup file ..\setup.exe to the specified folder

## PARAMETERS

### -DestinationPath
Destination folder

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -SetupFile
The main setupfile in the source directory

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

### -SourcePath
The directory containing all the installation files

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

### System.Object
## NOTES

## RELATED LINKS
