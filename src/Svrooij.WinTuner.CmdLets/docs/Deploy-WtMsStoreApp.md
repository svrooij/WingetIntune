---
external help file: Svrooij.WinTuner.CmdLets.dll-Help.xml
Module Name: Svrooij.WinTuner.CmdLets
online version: https://wintuner.app/docs/wintuner-powershell/Deploy-WtMsStoreApp
schema: 2.0.0
---

# Deploy-WtMsStoreApp

## SYNOPSIS
Create a MsStore app in Intune

## SYNTAX

### PackageId (Default)
```
Deploy-WtMsStoreApp [-PackageId] <String> [-Categories <String[]>] [-AvailableFor <String[]>]
 [-RequiredFor <String[]>] [-UninstallFor <String[]>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### SearchQuery
```
Deploy-WtMsStoreApp [-SearchQuery] <String> [-Categories <String[]>] [-AvailableFor <String[]>]
 [-RequiredFor <String[]>] [-UninstallFor <String[]>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Use this command to create an Microsoft Store app in Microsoft Intune

## EXAMPLES

### Example 1
```
PS C:\> Deploy-WtMsStoreApp -PackageId 9NZVDKPMR9RD
```

Add Firefox to Intune

## PARAMETERS

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

### -PackageId
The package id to upload to Intune.

```yaml
Type: String
Parameter Sets: PackageId
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

### -SearchQuery
Name of the app to look for, first match will be created.

```yaml
Type: String
Parameter Sets: SearchQuery
Aliases:

Required: True
Position: 0
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

### Microsoft.Graph.Beta.Models.WinGetApp
## NOTES

## RELATED LINKS
