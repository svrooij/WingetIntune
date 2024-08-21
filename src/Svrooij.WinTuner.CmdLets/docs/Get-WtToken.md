---
external help file: Svrooij.WinTuner.CmdLets.dll-Help.xml
Module Name: Svrooij.WinTuner.CmdLets
online version: https://wintuner.app/docs/wintuner-powershell/Get-WtToken
schema: 2.0.0
---

# Get-WtToken

## SYNOPSIS
Get a token for graph

## SYNTAX

```
Get-WtToken [-DecodeToken] [-ShowToken] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
This command will get a token for the graph api. The token is cached, so you can call this as often as you want.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-WtToken -DecodeToken | Set-Clipboard
```

Get token, show details and copy to clipboard

## PARAMETERS

### -DecodeToken
Decode the token

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

### -ShowToken
Output the token to the logs?

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

### None

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS

[https://wintuner.app/docs/wintuner-powershell/Get-WtToken](https://wintuner.app/docs/wintuner-powershell/Get-WtToken)

