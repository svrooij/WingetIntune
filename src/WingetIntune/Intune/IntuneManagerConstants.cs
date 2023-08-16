using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetIntune.Intune;
internal class IntuneManagerConstants
{
    internal const string PsCommandTemplate = @"$procOutput = & {command}
if($procOutput -is [array]) {
    $lastRow = $procOutput[$wingetOutput.Length -1]
    if ($lastRow.Contains(""{success}"")) {
        Write-Host ""{message}""
        Exit 0
    }
}
Write-Host ""Command Unsuccesful""
Write-Host ""$procOutput""
Exit 5
";

    internal const string PsDetectionCommandTemplate = @"$packageId = ""{packageId}""
$version = ""{version}""

$wingetOutput = & ""winget"" ""list"" ""--id"" $packageId ""--exact"" ""--disable-interactivity"" ""--accept-source-agreements""

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
