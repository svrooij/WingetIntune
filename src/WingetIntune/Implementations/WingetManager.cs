using Microsoft.Extensions.Logging;
using WingetIntune.Models;

namespace WingetIntune;

public partial class WingetManager : IWingetRepository
{
    private readonly ILogger<WingetManager> logger;
    private readonly IProcessManager processManager;

    public WingetManager(ILogger<WingetManager> logger, IProcessManager processManager)
    {
        this.logger = logger;
        this.processManager = processManager;
    }

    public async Task<Models.IsInstalledResult> CheckInstalled(string id, string? version, CancellationToken cancellationToken = default)
    {
        LogCheckInstalled(id, version);
        var result = await processManager.RunProcessAsync("winget", $"list --id {id} --exact --disable-interactivity --accept-source-agreements", cancellationToken);

        if (result.ExitCode != 0)
        {
            var exception = CreateExceptionForFailedProcess(result);
            LogErrorCheckInstalled(exception, id, version, result.Error);
            return IsInstalledResult.Error;
        }

        if (result.Output.Contains($"{id} "))
        {
            if (string.IsNullOrWhiteSpace(version) || result.Output.Contains($"{id} {version} "))
            {
                return IsInstalledResult.Installed;
            }
            else
            {
                return IsInstalledResult.UpgradeAvailable;
            }
        }
        return IsInstalledResult.NotInstalled;
    }

    public async Task<PackageInfo> GetPackageInfoAsync(string id, string? version, string? source, CancellationToken cancellationToken = default)
    {
        // Show package info from winget like the Install command
        LogGetPackageInfo(id, version);
        var args = new List<string>
        {
            "show",
            "--id",
            id
        };
        if (!string.IsNullOrEmpty(version))
        {
            args.Add("--version");
            args.Add(version);
        }
        if (!string.IsNullOrEmpty(source))
        {
            args.Add("--source");
            args.Add(source);
        }
        args.Add("--exact");
        args.Add("--accept-source-agreements");
        args.Add("--disable-interactivity");
        var result = await processManager.RunProcessAsync("winget", string.Join(" ", args), cancellationToken);
        if (result.ExitCode != 0)
        {
            var exception = CreateExceptionForFailedProcess(result);
            LogErrorGetPackageInfo(exception, id, version, result.Error);
            throw exception;
        }

        return Models.PackageInfo.Parse(result.Output);
    }

    private static Exception CreateExceptionForFailedProcess(ProcessResult processResult)
    {
        if (processResult.ExitCode == 0)
        {
            throw new ArgumentException("Process exited with exitcode 0");
        }

        var exception = new Exception("Winget exited with non-zero exitcode");
        exception.Data.Add("ExitCode", processResult.ExitCode);
        exception.Data.Add("Error", processResult.Error);
        return exception;
    }

    public async Task<ProcessResult> Install(string id, string? version, string? source, bool force, CancellationToken cancellationToken = default)
    {
        LogInstall(id, version);
        var args = new List<string>
        {
            "install",
            "--id",
            id
        };
        if (!string.IsNullOrEmpty(version))
        {
            args.Add("--version");
            args.Add(version);
        }
        if (!string.IsNullOrEmpty(source))
        {
            args.Add("--source");
            args.Add(source);
        }
        if (force)
        {
            args.Add("--force");
        }
        args.Add("--silent");
        args.Add("--accept-package-agreements");
        args.Add("--accept-source-agreements");
        args.Add("--disable-interactivity");
        return await processManager.RunProcessAsync("winget", string.Join(" ", args), cancellationToken, true);
    }

    public async Task<ProcessResult> Upgrade(string id, string? version, string? source, bool force, CancellationToken cancellationToken = default)
    {
        LogUpgrade(id, version);
        var args = new List<string>
        {
            "upgrade",
            "--id",
            id
        };
        if (!string.IsNullOrEmpty(version))
        {
            args.Add("--version");
            args.Add(version);
        }
        if (!string.IsNullOrEmpty(source))
        {
            args.Add("--source");
            args.Add(source);
        }
        if (force)
        {
            args.Add("--force");
        }
        args.Add("--silent");
        args.Add("--accept-package-agreements");
        args.Add("--accept-source-agreements");
        args.Add("--disable-interactivity");
        return await processManager.RunProcessAsync("winget", string.Join(" ", args), cancellationToken, true);
    }

    // Console.WriteLine($"Checking if package {id} {version} is installed");
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Checking if package {id} {version} is installed")]
    private partial void LogCheckInstalled(string id, string? version);

    // Console.WriteLine($"Upgrading package {id} {version}");
    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Upgrading package {id} {version}")]
    private partial void LogUpgrade(string id, string? version);

    // Console.WriteLine($"Installing package {id} {version}");
    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Installing package {id} {version}")]
    private partial void LogInstall(string id, string? version);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Getting package info for {id} {version}")]
    private partial void LogGetPackageInfo(string id, string? version);

    [LoggerMessage(EventId = 100, Level = LogLevel.Warning, Message = "Error getting package info for {id} {version}:\r\n{error}")]
    private partial void LogErrorGetPackageInfo(Exception exception, string id, string? version, string error);

    [LoggerMessage(EventId = 101, Level = LogLevel.Warning, Message = "Error checking installed {id} {version}:\r\n{error}")]
    private partial void LogErrorCheckInstalled(Exception exception, string id, string? version, string error);
}