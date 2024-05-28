---
external help file: Svrooij.WinTuner.CmdLets.dll-Help.xml
Module Name: Svrooij.WinTuner.CmdLets
online version: https://wintuner.app/docs/wintuner-powershell/Deploy-WtWin32App
schema: 2.0.0
---

# Deploy-WtWin32App

## SYNOPSIS
Create a Win32Lob app in Intune

## SYNTAX

### Win32LobApp (Default)
```
Deploy-WtWin32App [-App] <Win32LobApp> [-IntuneWinFile] <String> [[-LogoPath] <String>]
 [-OverrideAppName <String>] [-GraphId <String>] [[-UseManagedIdentity] <Boolean>]
 [-UseDefaultAzureCredential <Boolean>] [[-Token] <String>] [-NoBroker <Boolean>] [[-Username] <String>]
 [[-TenantId] <String>] [[-ClientId] <String>] [-ClientSecret <String>] [-Scopes <String[]>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### WinGet
```
Deploy-WtWin32App [-PackageId] <String> [-Version] <String> [-RootPackageFolder] <String>
 [-OverrideAppName <String>] [-GraphId <String>] [[-UseManagedIdentity] <Boolean>]
 [-UseDefaultAzureCredential <Boolean>] [[-Token] <String>] [-NoBroker <Boolean>] [[-Username] <String>]
 [[-TenantId] <String>] [[-ClientId] <String>] [-ClientSecret <String>] [-Scopes <String[]>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### PackageFolder
```
Deploy-WtWin32App [-PackageFolder] <String> [-OverrideAppName <String>] [-GraphId <String>]
 [[-UseManagedIdentity] <Boolean>] [-UseDefaultAzureCredential <Boolean>] [[-Token] <String>]
 [-NoBroker <Boolean>] [[-Username] <String>] [[-TenantId] <String>] [[-ClientId] <String>]
 [-ClientSecret <String>] [-Scopes <String[]>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Use this command to upload an intunewin package to Microsoft Intune as a new Win32LobApp.

## EXAMPLES

### Example 1
```
PS C:\> Deploy-WtWin32App -PackageFolder C:\Tools\packages\JanDeDobbeleer.OhMyPosh\19.5.2 -Username admin@myofficetenant.onmicrosoft.com
```

Upload a pre-packaged application, from just it's folder, using interactive authentication

## PARAMETERS

### -App
The App configuration you want to create

```yaml
Type: Win32LobApp
Parameter Sets: Win32LobApp
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -ClientId
(optionally) Use a different client ID, apart from the default configured one.

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

### -IntuneWinFile
The .intunewin file that should be added to this app

```yaml
Type: String
Parameter Sets: Win32LobApp
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -LogoPath
Load the logo from file

```yaml
Type: String
Parameter Sets: Win32LobApp
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: True (ByValue)
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

### -TenantId
Specify the tenant ID, if you want to use another tenant then your home tenant

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

### -ClientSecret
Specify the client secret, mandatory for Client Credentials flow. Loaded from `AZURE_CLIENT_SECRET`

```yaml
Type: String
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
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -GraphId
Graph ID of the app to supersede

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OverrideAppName
Override the name of the app in Intune

```yaml
Type: String
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

### Microsoft.Graph.Beta.Models.Win32LobApp
### System.String
## OUTPUTS

### Microsoft.Graph.Beta.Models.Win32LobApp
## NOTES

## RELATED LINKS
