using System.Reflection;

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

    internal static string GetPsDetectionCommand(string packageId, string version)
    {
        var script = getResourceScript("WingetDetection.ps1");
        return script.Replace("{packageId}", packageId).Replace("{version}", version);
    }

    internal static string GetPsGetWingetCmd()
    {
        return getResourceScript("FunctionGetWingetCmd.ps1");
    }

    private static string getResourceScript(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"WingetIntune.Scripts.{filename}";
        using (Stream stream = assembly.GetManifestResourceStream(resourceName)!)
        using (StreamReader reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
}
