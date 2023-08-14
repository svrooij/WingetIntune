Connect-MSIntuneGraph -Interactive -TenantID xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
# Get MSI meta data from .intunewin file
$IntuneWinFile = "C:\Tools\packages\Microsoft.AzureCLI\2.51.0\azure-cli-2.51.0.intunewin"
$IntuneWinMetaData = Get-IntuneWin32AppMetaData -FilePath $IntuneWinFile

# Create custom display name like 'Name' and 'Version'
$DisplayName = $IntuneWinMetaData.ApplicationInfo.Name + " " + $IntuneWinMetaData.ApplicationInfo.MsiInfo.MsiProductVersion


# Create MSI detection rule
$DetectionRuleArguments = @{
    "ProductCode" = $IntuneWinMetaData.ApplicationInfo.MsiInfo.MsiProductCode
    "ProductVersionOperator" = "greaterThanOrEqual"
    "ProductVersion" = $IntuneWinMetaData.ApplicationInfo.MsiInfo.MsiProductVersion
}
$DetectionRule = New-IntuneWin32AppDetectionRuleMSI @DetectionRuleArguments


# Create operative system requirement rule
$RequirementRule = New-IntuneWin32AppRequirementRule -Architecture "All" -MinimumSupportedWindowsRelease "W10_1909"


# Create custom return code
$ReturnCode = New-IntuneWin32AppReturnCode -ReturnCode 1337 -Type retry


# Construct a table of default parameters for the Win32 app
$Win32AppArgs = @{
    "FilePath" = $IntuneWinFile
    "DisplayName" = $DisplayName
    "Description" = "App description"
    "Publisher" = "MSEndpointMgr"
    "AppVersion" = $IntuneWinMetaData.ApplicationInfo.MsiInfo.MsiProductVersion
    "InstallExperience" = "system"
    "RestartBehavior" = "suppress"
    "DetectionRule" = $DetectionRule
    "RequirementRule" = $RequirementRule
    "ReturnCode" = $ReturnCode
    "Verbose" = $true
}
Add-IntuneWin32App @Win32AppArgs