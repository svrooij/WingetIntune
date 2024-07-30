using Microsoft.Extensions.Logging;
using SvRooij.ContentPrep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingetIntune.Models;

namespace WingetIntune.Testing;
public class WindowsSandbox
{
    private readonly ILogger<WindowsSandbox> logger;
    private readonly Packager packager;
    private readonly IProcessManager processManager;

    public WindowsSandbox(ILoggerFactory loggerFactory, IProcessManager processManager)
    {
        this.logger = loggerFactory.CreateLogger<WindowsSandbox>();
        this.packager = new Packager(loggerFactory.CreateLogger<Packager>());
        this.processManager = processManager;
    }

    public async Task<string> PrepareSandboxFileForPackage(PackageInfo packageInfo, string intuneWinFile, string outputFolder, int? timeout = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Preparing sandbox file for {PackageId} {Version}", packageInfo.PackageIdentifier, packageInfo.Version);

        var installerFolder = Path.Combine(outputFolder, "installer");
        var logsFolder = Path.Combine(outputFolder, "logs");

        Directory.CreateDirectory(installerFolder);
        Directory.CreateDirectory(logsFolder);
        await packager.Unpack(intuneWinFile, installerFolder, cancellationToken);

        var sandboxFile = Path.Combine(outputFolder, "sandbox.wsb");
        await WriteSandboxConfig(sandboxFile, installerFolder, logsFolder);
        await WriteTestScript(installerFolder, packageInfo, timeout);

        return sandboxFile;
    }

    private static async Task WriteSandboxConfig(string sandboxFilename, string installerFolder, string logFolder)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("<Configuration>");
        // Mapped folders (installer and logs)
        // some logs are parsed to show the actual result.
        stringBuilder.AppendLine("  <MappedFolders>");
        stringBuilder.AppendLine("    <MappedFolder>");
        stringBuilder.AppendLine($"      <HostFolder>{installerFolder}</HostFolder>");
        stringBuilder.AppendLine("      <SandboxFolder>c:\\Users\\WDAGUtilityAccount\\Downloads\\installer</SandboxFolder>");
        stringBuilder.AppendLine("      <ReadOnly>true</ReadOnly>");
        stringBuilder.AppendLine("    </MappedFolder>");
        stringBuilder.AppendLine("    <MappedFolder>");
        stringBuilder.AppendLine($"      <HostFolder>{logFolder}</HostFolder>");
        stringBuilder.AppendLine("      <SandboxFolder>c:\\Users\\WDAGUtilityAccount\\Desktop\\logs</SandboxFolder>");
        stringBuilder.AppendLine("      <ReadOnly>false</ReadOnly>");
        stringBuilder.AppendLine("    </MappedFolder>");
        stringBuilder.AppendLine("  </MappedFolders>");

        // Startup command
        stringBuilder.AppendLine("  <LogonCommand>");
        stringBuilder.AppendLine("    <Command>c:\\Users\\WDAGUtilityAccount\\Downloads\\installer\\wintuner\\startup.cmd</Command>");
        stringBuilder.AppendLine("  </LogonCommand>");

        // Security settings
        stringBuilder.AppendLine("  <AudioInput>Disable</AudioInput>");
        stringBuilder.AppendLine("  <VideoInput>Disable</VideoInput>");
        // Not sure about this next one https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/windows-sandbox-configure-using-wsb-file#protected-client
        stringBuilder.AppendLine("  <ProtectedClient>Enabled</ProtectedClient>");
        stringBuilder.AppendLine("  <PrinterRedirection>Disable</PrinterRedirection>");
        stringBuilder.AppendLine("  <ClipboardRedirection>Disable</ClipboardRedirection>");
        stringBuilder.AppendLine("</Configuration>");

        await File.WriteAllTextAsync(sandboxFilename, stringBuilder.ToString());
    }

    private static async Task WriteTestScript(string installerFolder, PackageInfo packageInfo, int? timeout)
    {
        var arguments = packageInfo.InstallCommandLine?.Replace($"\"{packageInfo.InstallerFilename}\" ", "");
        // Create a batch script that will run a powershell script (Execution policy stuff...)
        var sb = new StringBuilder();
        sb.AppendLine("@echo off");
        sb.AppendLine("start /wait /low powershell.exe -ExecutionPolicy Bypass -File \"C:\\Users\\WDAGUtilityAccount\\Downloads\\installer\\wintuner\\install.ps1\"");
        sb.AppendLine();
        Directory.CreateDirectory(Path.Combine(installerFolder, "wintuner"));
        await File.WriteAllTextAsync(Path.Combine(installerFolder, "wintuner", "startup.cmd"), sb.ToString());

        sb.Clear();

        // Create the powershell script that will install the app
        // and collect the installed apps
        // This script will also shutdown the sandbox after the installation (if a timeout above -1 is provided)

        sb.AppendLine("Start-Transcript -Path c:\\Users\\WDAGUtilityAccount\\Desktop\\logs\\wintuner.log -Append -Force");
        sb.AppendLine("Write-Host \"Starting installation\"");
        sb.AppendLine($"Write-Host \"Installer: {packageInfo.InstallerFilename}\"");
        sb.AppendLine($"Write-Host \"Arguments: {arguments}\"");

        // execute the installer and capture the exit code in powershell
        sb.AppendLine($"& c:\\Users\\WDAGUtilityAccount\\Downloads\\installer\\{packageInfo.InstallerFilename} {arguments}");
        //sb.AppendLine("& cmd exit /b 5"); // This is a dummy command to test the exit code
        sb.AppendLine("$exitCode = $LASTEXITCODE");
        sb.AppendLine("Write-Host \"Installer finished with exitcode $exitCode\"");

        // write the exit code to a file
        sb.AppendLine("$exitCode | Out-File -FilePath c:\\Users\\WDAGUtilityAccount\\Desktop\\logs\\exitcode.txt");
        sb.AppendLine("Write-Host \"App installed, collecting installed apps\"");
        sb.AppendLine("$apps = $(Get-WmiObject -Class Win32_InstalledWin32Program | Select-Object -Property Version,Vendor,Name)");
        sb.AppendLine("$apps | Format-Table -AutoSize");
        sb.AppendLine("$apps | Export-Csv -Path c:\\Users\\WDAGUtilityAccount\\Desktop\\logs\\installed.csv -NoTypeInformation");
        sb.AppendLine("Write-Host \"Installed apps collected\"");
        sb.AppendLine("Stop-Transcript");
        if (timeout is not null && timeout >= 0)
        {
            if (timeout > 0) // Cancelable shutdown
            {
                sb.AppendLine($"shutdown /s /t {timeout}");
                sb.AppendLine($"Write-Host \"Closing sandbox in {timeout} seconds unless you press a button\"");
                sb.AppendLine("Read-Host");
                sb.AppendLine("shutdown /a");
            }
            else // Immediate shutdown
            {
                sb.AppendLine($"shutdown /s /t {timeout}");
            }
        }
        // Exit with the exit code of the installer (not sure if that does anything)
        sb.AppendLine("exit $exitCode");
        await File.WriteAllTextAsync(Path.Combine(installerFolder, "wintuner", "install.ps1"), sb.ToString());
    }

    public async Task<SandboxResult?> RunSandbox(string sandboxFile, bool cleanup, CancellationToken cancellationToken)
    {
        logger.LogInformation("Running sandbox {sandboxFile}", sandboxFile);
        var processResult = await processManager.RunProcessAsync("WindowsSandbox.exe", sandboxFile, cancellationToken);
        logger.LogInformation("Sandbox exited with exitcode {exitCode}", processResult.ExitCode);
        bool shouldProcess = true;
        SandboxResult? result = null;
        if (processResult.ExitCode == -2147024713)
        {
            logger.LogWarning("Sandbox failed to start, this is likely because the host does not support virtualization or already started");
            shouldProcess = false;
        }
        await Task.Delay(1500, cancellationToken);

        if (shouldProcess)
        {
            var logDirectory = Path.Combine(Path.GetDirectoryName(sandboxFile)!, "logs");
            var logFile = Path.Combine(logDirectory, "wintuner.log");
            var exitCodeFile = Path.Combine(logDirectory, "exitcode.txt");
            result = new SandboxResult
            {
                ExitCode = File.Exists(exitCodeFile) && int.TryParse(await File.ReadAllTextAsync(exitCodeFile), out int exitCode) ? exitCode : 0,
                Log = File.Exists(logFile) ? await File.ReadAllTextAsync(logFile) : null,
                InstalledApps = await ParseInstalledApps(Path.Combine(logDirectory, "installed.csv"))
            };
        }

        if (cleanup)
        {
            logger.LogDebug("Cleaning up sandbox files");
            Directory.Delete(Path.GetDirectoryName(sandboxFile)!, true);
        }
        return result;

    }

    private async Task<IEnumerable<SandboxInstalledApps>?> ParseInstalledApps(string filename)
    {

        // the file is a csv with headers Version,Vendor,Name and uses , as separator
        // Parse the file if it exists
        if (!File.Exists(filename))
        {
            logger.LogWarning("Installed apps file not found {filename}", filename);
            return null;
        }

        var lines = await File.ReadAllLinesAsync(filename);
        if (lines.Length < 2)
        {
            logger.LogWarning("Installed apps file is empty {filename}", filename);
            return null;
        }

        return lines.Skip(1).Select(l =>
        {
            var parts = l.Split(',');
            return new SandboxInstalledApps
            {
                Version = parts[0].Trim('"'),
                Vendor = parts[1].Trim('"'),
                Name = parts[2].Trim('"')
            };
        });
    }

    public class SandboxResult
    {
        public int ExitCode { get; set; } = 0;
        public string? Log { get; set; }
        public IEnumerable<SandboxInstalledApps>? InstalledApps { get; set; }

        public override string ToString()
        {
            return InstalledApps?.Count() > 0 ? $"ExitCode: {ExitCode}, Installed apps {string.Join(", ", InstalledApps.Select(i => i.Name))}" : $"Exit code: {ExitCode}";
        }

    }

    public class SandboxInstalledApps
    {
        public string? Version { get; set; }
        public string? Vendor { get; set; }
        public string? Name { get; set; }

        public override string ToString()
        {
            return $"{Name} by {Vendor}";
        }
    }
}
