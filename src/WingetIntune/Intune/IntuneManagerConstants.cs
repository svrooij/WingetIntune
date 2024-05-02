namespace WingetIntune.Intune;

internal class IntuneManagerConstants
{
    internal const string AllUsers = "AllUsers";
    internal const string AllDevices = "AllDevices";

    internal const string PsCommandTemplate = @"$packageId = ""{packageId}""
$action = ""{action}""

Start-Transcript -Path ""$env:ProgramData\Microsoft\IntuneManagementExtension\Logs\$packageId-$action.log"" -Force
Write-Host ""Starting $packageId $action""

Function Get-WingetCmd {

    $WingetCmd = $null
    #Get WinGet Path

    try {
        # Get Admin Context Winget Location
        $WingetInfo = (Get-Item ""$env:ProgramFiles\WindowsApps\Microsoft.DesktopAppInstaller_*_8wekyb3d8bbwe\winget.exe"").VersionInfo | Sort-Object -Property FileVersionRaw
        # if multiple versions, pick most recent one
        $WingetCmd = $WingetInfo[-1].FileName
    }
    catch {
        #Get User context Winget Location
        if (Test-Path ""$env:LocalAppData\Microsoft\WindowsApps\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\winget.exe"")
        {
            $WingetCmd =""$env:LocalAppData\Microsoft\WindowsApps\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\winget.exe""
        } else {
            Write-Host ""winget not detected""
            Exit 1
        }
    }
    Write-Host ""Winget location: $WingetCmd""
    return $WingetCmd
}

$procOutput = & {command}
if($procOutput -is [array]) {
    $lastRow = $procOutput[$wingetOutput.Length -1]
    if ($lastRow.Contains(""{success}"")) {
        Write-Host ""{message}""
        Exit 0
    }
}
Write-Host ""Command Unsuccessful""
Write-Host ""$procOutput""
Exit 5
";

    internal const string PsDetectionCommandTemplate = @"$packageId = ""{packageId}""
$version = ""{version}""
 
Start-Transcript -Path ""$env:ProgramData\Microsoft\IntuneManagementExtension\Logs\$packageId-detection.log"" -Force
Write-Host ""Starting $packageId detection""

# Need to get the full path of winget, because detection script is run in a different context
Function Get-WingetCmd {

    $WingetCmd = $null
    #Get WinGet Path

    try {
        # Get Admin Context Winget Location
        $WingetInfo = (Get-Item ""$env:ProgramFiles\WindowsApps\Microsoft.DesktopAppInstaller_*_8wekyb3d8bbwe\winget.exe"").VersionInfo | Sort-Object -Property FileVersionRaw
        # if multiple versions, pick most recent one
        $WingetCmd = $WingetInfo[-1].FileName
    }
    catch {
        #Get User context Winget Location
        if (Test-Path ""$env:LocalAppData\Microsoft\WindowsApps\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\winget.exe"")
        {
            $WingetCmd =""$env:LocalAppData\Microsoft\WindowsApps\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\winget.exe""
        }
    }
    Write-Host ""Winget location: $WingetCmd""
    return $WingetCmd
}

$wingetCmd = Get-WingetCmd
if ($null -eq $wingetCmd) {
    Write-Host ""winget not detected""
    Exit 1
}

$wingetOutput = & $wingetCmd ""list"" ""--id"" $packageId ""--exact"" ""--disable-interactivity"" ""--accept-source-agreements""

if($wingetOutput -is [array]) {
    $lastRow = $wingetOutput[$wingetOutput.Length -1]
    if ($lastRow.Contains(""$packageId $version"")) {
        Write-Host ""$($packageId) version $($version) is installed""
        Exit 0
    } elseif ($lastRow.Contains($packageId)) {
        [reflection.assembly]::LoadWithPartialName(""System.Version"")
        $i = $lastRow.IndexOf("" $packageId "") + $packageId.Length + 1
        $nextSpace = $lastRow.IndexOf("" "", $i + 1)
        $installedVersion = $lastRow.Substring($i+1, $nextSpace - $i -1)
        $versionExpected = New-Object System.Version($version)
        $versionInstalled = New-Object System.Version($installedVersion)
        Write-Host ""$($packageId) version $($installedVersion) is installed""
        $result = $versionExpected.CompareTo($versionInstalled);
        if (1 -eq $result) {
            Write-Host ""Installed version is lower""
            Exit 5
        }
        Exit 0
    }
}

Write-Host ""$($packageId) not detected using winget""
Exit 10";
}
