using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingetIntune.Models;
namespace WingetIntune;

internal class WingetManager
{
    internal static async Task<Models.IsInstalledResult> CheckInstalled(string id, string? version, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Checking if package {id} {version} is installed");
        var result = await ProcessManager.RunProcessAsync("winget", $"list --id {id} --exact --disable-interactivity --accept-source-agreements", cancellationToken);
        
        if (result.ExitCode != 0)
        {
            Console.Error.WriteLine(result.Error);
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

    internal static async Task<Models.PackageInfo> GetPackageInfoAsync(string id, string? version, string? source, CancellationToken cancellationToken = default)
    {
        // Show package info from winget like the Install command
        var args = new List<string>();
        args.Add("show");
        args.Add("--id");
            args.Add(id);
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
        var result = await ProcessManager.RunProcessAsync("winget", string.Join(" ", args), cancellationToken);
        if (result.ExitCode != 0)
        {
            Console.Error.WriteLine(result.Error);
            throw new Exception("Winget threw an exception");
        }

        return Models.PackageInfo.Parse(result.Output);
    }

    internal static async Task<ProcessResult> Install(string id, string? version, string? source, bool force, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Installing package {id} {version}");
        var args = new List<string>();
        args.Add("install");
        args.Add("--id");
        args.Add(id);
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
        args.Add("--accept-source-agreements");
        args.Add("--disable-interactivity");
        return await ProcessManager.RunProcessAsync("winget", string.Join(" ", args), cancellationToken, true);
    }

    internal static async Task<ProcessResult> Upgrade(string id, string? version, string? source, bool force, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Upgrading package {id} {version}");
        var args = new List<string>();
        args.Add("upgrade");
        args.Add("--id");
        args.Add(id);
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
        return await ProcessManager.RunProcessAsync("winget", string.Join(" ", args), cancellationToken, true);
    }

}




