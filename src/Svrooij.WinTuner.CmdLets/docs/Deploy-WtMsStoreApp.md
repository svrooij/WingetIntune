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
Deploy-WtMsStoreApp [-PackageId] <String> [[-UseManagedIdentity] <Boolean>]
 [[-UseDefaultAzureCredential] <Boolean>] [[-Token] <String>] [-NoBroker <Boolean>] [[-Username] <String>]
 [[-TenantId] <String>] [[-ClientId] <String>] [[-ClientSecret] <String>] [-Scopes <String[]>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### SearchQuery
```
Deploy-WtMsStoreApp [-SearchQuery] <String> [[-UseManagedIdentity] <Boolean>]
 [[-UseDefaultAzureCredential] <Boolean>] [[-Token] <String>] [-NoBroker <Boolean>] [[-Username] <String>]
 [[-TenantId] <String>] [[-ClientId] <String>] [[-ClientSecret] <String>] [-Scopes <String[]>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Use this command to create an Microsoft Store app in Microsoft Intune

## EXAMPLES

### Example 1
```powershell
PS C:\> Deploy-WtMsStoreApp -PackageId 9NZVDKPMR9RD -Username admin@myofficetenant.onmicrosoft.com
```

Add Firefox to Intune, using interactive authentication

## PARAMETERS

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

### Microsoft.Graph.Beta.Models.WinGetApp

## NOTES

## RELATED LINKS
