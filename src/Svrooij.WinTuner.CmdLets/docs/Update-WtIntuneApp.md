---
external help file: Svrooij.WinTuner.CmdLets.dll-Help.xml
Module Name: Svrooij.WinTuner.CmdLets
online version: https://wintuner.app/docs/wintuner-powershell/Update-WtIntuneApp
schema: 2.0.0
---

# Update-WtIntuneApp

## SYNOPSIS
Update an app in Intune

## SYNTAX

```
Update-WtIntuneApp -AppId <String> [-Categories <String[]>] [-AvailableFor <String[]>]
 [-RequiredFor <String[]>] [-UninstallFor <String[]>] [-EnableAutoUpdate <Boolean>]
 [[-UseManagedIdentity] <Boolean>] [[-UseDefaultAzureCredential] <Boolean>] [[-Token] <String>]
 [-NoBroker <Boolean>] [[-Username] <String>] [[-TenantId] <String>] [[-ClientId] <String>]
 [[-ClientSecret] <String>] [-Scopes <String[]>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Update the assignments and/or categories for an app in Intune.

## EXAMPLES

### Example 1
```
PS C:\> Update-WtIntuneApp -AppId "1450c17d-aee5-4bef-acf9-9e0107d340f2" -UseDefaultCredentials -Categories "Productivity","Business" -AvailableFor "AllUsers"
```

Update the categories of an app and make it available for all users

## PARAMETERS

### -AppId
Id of the app in Intune

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
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

### -ClientId
Specify the client ID, optional for interactive, mandatory for Client Credentials flow.
Loaded from \`AZURE_CLIENT_ID\`

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 27
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClientSecret
Specify the client secret, mandatory for Client Credentials flow.
Loaded from \`AZURE_CLIENT_SECRET\`

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 28
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

### -TenantId
Specify the tenant ID, optional for interactive, mandatory for Client Credentials flow.
Loaded from \`AZURE_TENANT_ID\`

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 26
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Token
Use a token from another source to connect to Intune

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 22
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

### -UseDefaultAzureCredential
Use default Azure Credentials from Azure.Identity to connect to Intune

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: 21
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UseManagedIdentity
Use a managed identity to connect to Intune

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: 20
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Username
Use a username to trigger interactive login or SSO

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 25
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

### -EnableAutoUpdate
Enable auto update for the app

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoBroker
Disable Windows authentication broker

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Scopes
Specify the scopes to request, default is `DeviceManagementConfiguration.ReadWrite.All`, `DeviceManagementApps.ReadWrite.All`

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### Microsoft.Graph.Beta.Models.MobileApp
## NOTES

## RELATED LINKS
