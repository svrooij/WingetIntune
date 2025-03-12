# WinTuner - Get-WingetCmd function https://wintuner.app

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

