$packageId = "Microsoft.AzureCLI"
$version = "2.51.0"

$wingetOutput = & "winget" "list" "--id" $packageId "--exact" "--disable-interactivity" "--accept-source-agreements"

if($wingetOutput -is [array]) {
    $lastRow = $wingetOutput[$wingetOutput.Length -1]
    if ($lastRow.Contains("$packageId $version")) {
        Write-Host "$($packageId) version $($version) is installed"
        Exit 0
    } elseif ($lastRow.Contains($packageId)) {
        Write-Host "$($packageId) is installed but not the correct version"
        Write-Host "Winget output: $($lastRow)"
        Exit 5
    }
}

Write-Host "$($packageId) not detected using winget"
Exit 10