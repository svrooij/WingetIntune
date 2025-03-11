# WinTuner - Winget Detection script https://wintuner.app
# 
# Script parameters in `{parameter_name}`
# packageId - The package id of the application to be detected
# version - The version of the application to be detected

# --------------------------- Start parameters -------------------------------
$packageId = "{packageId}"
$version = "{version}"
# --------------------------- End parameters ---------------------------------

# ------------------------------------Start script, do not edit below -----------------------------------------
Start-Transcript -Path "$env:ProgramData\Microsoft\IntuneManagementExtension\Logs\$packageId-detection.log" -Force
Write-Host "Starting $packageId detection"

# Need to get the full path of winget, because detection script is run in a different context
Function Get-WingetCmd {

    $WingetCmd = $null
    #Get WinGet Path

    try {
        # Get Admin Context Winget Location
        $WingetInfo = (Get-Item "$env:ProgramFiles\WindowsApps\Microsoft.DesktopAppInstaller_*_8wekyb3d8bbwe\winget.exe").VersionInfo | Sort-Object -Property FileVersionRaw
        # if multiple versions, pick most recent one
        $WingetCmd = $WingetInfo[-1].FileName
    }
    catch {
        #Get User context Winget Location
        if (Test-Path "$env:LocalAppData\Microsoft\WindowsApps\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\winget.exe")
        {
            $WingetCmd ="$env:LocalAppData\Microsoft\WindowsApps\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\winget.exe"
        }
    }
    Write-Host "Winget location: $WingetCmd"
    return $WingetCmd
}

$wingetCmd = Get-WingetCmd
if ($null -eq $wingetCmd) {
    Write-Host "winget not detected"
    Write-Host "Exiting with code 1"
    Exit 1
}

$wingetOutput = & $wingetCmd "list" "--id" $packageId "--exact" "--accept-source-agreements"

if($wingetOutput -is [array]) {
    $lastRow = $wingetOutput[$wingetOutput.Length -1]
    if ($lastRow.Contains("$packageId $version")) {
        Write-Host "$($packageId) version $($version) is installed"
        Write-Host "Exiting with code 0"
        Exit 0
    } elseif ($lastRow.Contains($packageId)) {
        [reflection.assembly]::LoadWithPartialName("System.Version")
        $i = $lastRow.IndexOf(" $packageId ") + $packageId.Length + 1
        $nextSpace = $lastRow.IndexOf(" ", $i + 1)
        $installedVersion = $lastRow.Substring($i+1, $nextSpace - $i -1)
        $versionExpected = New-Object System.Version($version)
        $versionInstalled = New-Object System.Version($installedVersion)
        Write-Host "$($packageId) version $($installedVersion) is installed"
        $result = $versionExpected.CompareTo($versionInstalled);
        if (1 -eq $result) {
            Write-Host "Installed version is lower"
            Write-Host "Exiting with code 5"
            Exit 5
        }
        Write-Host "Exiting with code 0"
        Exit 0
    }
}

Write-Host "$($packageId) not detected using winget"
Write-Host "Exiting with code 10"
Exit 10