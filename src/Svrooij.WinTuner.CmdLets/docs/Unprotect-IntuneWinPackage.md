---
external help file: Svrooij.WinTuner.CmdLets.dll-Help.xml
Module Name: Svrooij.WinTuner.CmdLets
online version: https://wintuner.app/docs/related/content-prep-tool#unlock-intunewinpackage
schema: 2.0.0
---

# Unprotect-IntuneWinPackage

## SYNOPSIS
Decrypt an IntuneWin package

## SYNTAX

```
Unprotect-IntuneWinPackage [-SourceFile] <String> [-DestinationPath] <String>
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Decrypt IntuneWin files, based on this post https://svrooij.io/2023/10/09/decrypting-intunewin-files/

## EXAMPLES

### Example 1
```
PS C:\> Unprotect-IntuneWinPackage -SourceFile C:\Temp\Source\MyApp.intunewin -DestinationPath C:\Temp\Destination
```

## PARAMETERS

### -DestinationPath
Destination folder

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

### -SourceFile
The location of the .intunewin file

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
