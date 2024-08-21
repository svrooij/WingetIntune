---
external help file: Svrooij.WinTuner.CmdLets.dll-Help.xml
Module Name: Svrooij.WinTuner.CmdLets
online version: https://wintuner.app/docs/wintuner-powershell/Connect-WtWinTuner
schema: 2.0.0
---

# Connect-WtWinTuner

## SYNOPSIS
Connect to Intune

## SYNTAX

### Interactive (Default)
```
Connect-WtWinTuner [-NoBroker] [-Username] <String> [[-TenantId] <String>] [[-ClientId] <String>]
 [[-Scopes] <String[]>] [-Test] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### UseManagedIdentity
```
Connect-WtWinTuner [-UseManagedIdentity] [[-Scopes] <String[]>] [-Test] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### UseDefaultCredentials
```
Connect-WtWinTuner [-UseDefaultCredentials] [[-Scopes] <String[]>] [-Test] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### Token
```
Connect-WtWinTuner [-Token] <String> [[-Scopes] <String[]>] [-Test] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### ClientCredentials
```
Connect-WtWinTuner [-TenantId] <String> [-ClientId] <String> [-ClientSecret] <String> [[-Scopes] <String[]>]
 [-Test] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
A separate command to select the correct authentication provider, you no longer have to provide the auth parameters with each command.

## EXAMPLES

### Example 1

```powershell
PS C:\> Connect-WtWinTuner -Username "youruser@contoso.com"
```

Connect using interactive authentication

### Example 2

```powershell
Connect-WtWinTuner -UseManagedIdentity az login & Connect-WtWinTuner -UseDefaultCredentials
```

Connect using managed identity Connect using default credentials

## PARAMETERS

### -ClientId
Specify the client ID, mandatory for Client Credentials flow.
Loaded from \`AZURE_CLIENT_ID\`

```yaml
Type: String
Parameter Sets: Interactive
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: ClientCredentials
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClientSecret
Specify the client secret.
Loaded from \`AZURE_CLIENT_SECRET\`

```yaml
Type: String
Parameter Sets: ClientCredentials
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoBroker
Disable Windows authentication broker

```yaml
Type: SwitchParameter
Parameter Sets: Interactive
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Scopes
Specify the scopes to request, default is \`DeviceManagementConfiguration.ReadWrite.All\`, \`DeviceManagementApps.ReadWrite.All\`

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 10
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TenantId
Specify the tenant ID, optional.
Loaded from \`AZURE_TENANT_ID\`

```yaml
Type: String
Parameter Sets: Interactive
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: ClientCredentials
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Test
Try to get a token after connecting, useful for testing.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 11
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Token
Use a token from another source to connect to Intune, this is the least preferred way to use

```yaml
Type: String
Parameter Sets: Token
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UseDefaultCredentials
Use default Azure Credentials from Azure.Identity to connect to Intune

```yaml
Type: SwitchParameter
Parameter Sets: UseDefaultCredentials
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UseManagedIdentity
Use a managed identity to connect to Intune

```yaml
Type: SwitchParameter
Parameter Sets: UseManagedIdentity
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Username
Use a username to trigger interactive login or SSO

```yaml
Type: String
Parameter Sets: Interactive
Aliases:

Required: True
Position: 0
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

[https://wintuner.app/docs/wintuner-powershell/Connect-WtWinTuner](https://wintuner.app/docs/wintuner-powershell/Connect-WtWinTuner)

