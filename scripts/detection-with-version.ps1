$packageId = "7zip.7zip"
$version = "23.02.0.0"

$wingetOutput = & "winget" "list" "--id" $packageId "--exact" "--disable-interactivity" "--accept-source-agreements"

if($wingetOutput -is [array]) {
    $lastRow = $wingetOutput[$wingetOutput.Length -1]
    if ($lastRow.Contains("$packageId $version")) {
        Write-Host "$($packageId) version $($version) is installed"
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
            Exit 5
        }
        Exit 0
    }
}

Write-Host "$($packageId) not detected using winget"
Exit 10