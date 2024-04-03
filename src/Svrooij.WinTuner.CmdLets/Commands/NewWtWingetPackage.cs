using Microsoft.Extensions.Logging;
using Svrooij.PowerShell.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Svrooij.WinTuner.CmdLets.Commands;
/// <summary>
/// <para type="synopsis">Create intunewin file from Winget installer</para>
/// <para type="description">Downloads the installer for the package and creates an `.intunewin` file for uploading in Intune.</para>
/// <para type="link" uri="https://wintuner.app/">Documentation</para>
/// </summary>
/// <example>
/// <para type="description">Package all files in C:\Temp\Source, with setup file ..\setup.exe to the specified folder</para>
/// <code>New-WingetPackage -PackageId JanDeDobbeleer.OhMyPosh -PackageFolder C:\Tools\Packages</code>
/// </example>
[Cmdlet(VerbsCommon.New, "WtWingetPackage")]
[OutputType(typeof(WingetIntune.Models.WingetPackage))]
public class NewWtWingetPackage : DependencyCmdlet<Startup>
{
    /// <summary>
    /// Package id to download
    /// </summary>
    [Parameter(
               Mandatory = true,
               Position = 0,
               ValueFromPipeline = true,
               ValueFromPipelineByPropertyName = true,
               HelpMessage = "The package id to download")]
    public string? PackageId { get; set; }

    /// <summary>
    /// The folder to store the package in
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 1,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The folder to store the package in")]
    public string PackageFolder { get; set; }

    /// <summary>
    /// The version to download (optional)
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 2,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The version to download (optional)")]
    public string? Version { get; set; }

    /// <summary>
    /// The folder to store temporary files in
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 3,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The folder to store temporary files in")]
    public string? TempFolder { get; set; } = Path.Combine(Path.GetTempPath(), "wintuner");

    [ServiceDependency]
    private ILogger<NewWtWingetPackage> logger;

    [ServiceDependency]
    private Winget.CommunityRepository.WingetRepository wingetRepository;

    [ServiceDependency]
    private WingetIntune.IWingetRepository repository;

    [ServiceDependency]
    private WingetIntune.IntuneManager intuneManager;

    /// <inheritdoc/>
    public override async Task ProcessRecordAsync(CancellationToken cancellationToken)
    {
        // Fix the package id casing.
        PackageId = await wingetRepository.GetPackageId(PackageId!, cancellationToken);
        if (string.IsNullOrEmpty(Version))
        {
            Version = await wingetRepository.GetLatestVersion(PackageId!, cancellationToken);
        }

        logger.LogInformation("Packaging package {PackageId} {Version}", PackageId, Version);

        var packageInfo = await repository.GetPackageInfoAsync(PackageId!, Version, source: "winget", cancellationToken: cancellationToken);

        if (packageInfo != null)
        {
            logger.LogDebug("Package {PackageId} {Version} from {Source}", packageInfo.PackageIdentifier, packageInfo.Version, packageInfo.Source);

            var package = await intuneManager.GenerateInstallerPackage(
                TempFolder!,
                PackageFolder,
                packageInfo,
                cancellationToken: cancellationToken);

            WriteObject(package);
        }
        else
        {
            logger.LogWarning("Package {PackageId} not found", PackageId);
        }
    }
}
