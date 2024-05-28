---
external help file: Svrooij.WinTuner.CmdLets.dll-Help.xml
Module Name: Svrooij.WinTuner.CmdLets
online version: https://wintuner.app/docs/wintuner-powershell/Search-WtWingetPackage
schema: 2.0.0
---

# Search-WtWinGetPackage

## SYNOPSIS
Search for packages in winget

## SYNTAX

```
Search-WtWinGetPackage [-PackageId] <String> [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Search for WinGet packages, but faster

## EXAMPLES

### Example 1
```
PS C:\> Search-WtWinGetPackage fire
```

Search for fire, did I tell you it's fast?

## PARAMETERS

### -PackageId
Part of the package ID, 2 characters minimum

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

### Winget.CommunityRepository.Models.WingetEntry[]
## NOTES

## RELATED LINKS
