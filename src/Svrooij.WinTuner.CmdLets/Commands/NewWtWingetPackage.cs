using Microsoft.Extensions.Logging;
using Svrooij.PowerShell.DependencyInjection;
using System.IO;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace Svrooij.WinTuner.CmdLets.Commands;
/// <summary>
/// <para type="synopsis">Create intunewin file from Winget installer</para>
/// <para type="description">Downloads the installer for the package and creates an `.intunewin` file for uploading in Intune.</para>
/// <para type="link" uri="https://wintuner.app/docs/wintuner-powershell/New-WtWingetPackage">Documentation</para> 
/// </summary>
/// <example>
/// <para type="description">Package all files in C:\Temp\Source, with setup file ..\setup.exe to the specified folder</para>
/// <code>New-WtWingetPackage -PackageId JanDeDobbeleer.OhMyPosh -PackageFolder C:\Tools\Packages</code>
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
        Mandatory = true,
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

    /// <summary>
    /// Pick this architecture
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 4,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "Pick this architecture")]
    public WingetIntune.Models.Architecture Architecture { get; set; } = WingetIntune.Models.Architecture.Unknown;

    /// <summary>
    /// The installer context
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 5,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The installer context")]
    public WingetIntune.Models.InstallerContext InstallerContext { get; set; } = WingetIntune.Models.InstallerContext.System;

    /// <summary>
    /// Package as script
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 6,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "Package WinGet script, instead of the actual installer. Helpful for installers that don't really work with WinTuner.")]
    public bool? PackageScript { get; set; }

    /// <summary>
    /// Desired locale
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 7,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The desired locale, if available (eg. 'en-US')")]
    public string? Locale { get; set; }

    /// <summary>
    /// Override the installer arguments
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 8,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Override the installer arguments")]
    public string? InstallerArguments { get; set; }

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
        PackageId = (await wingetRepository!.GetPackageId(PackageId!, cancellationToken)) ?? string.Empty;
        if (string.IsNullOrEmpty(PackageId))
        {
            logger.LogWarning("Package {PackageId} not found", PackageId);
            return;
        }
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
                new WingetIntune.Models.PackageOptions
                {
                    Architecture = Architecture,
                    InstallerContext = InstallerContext,
                    PackageScript = PackageScript ?? false,
                    Locale = Locale,
                    OverrideArguments = InstallerArguments
                },
                cancellationToken: cancellationToken);

            WriteObject(package);
        }
        else
        {
            logger.LogWarning("Package {PackageId} not found", PackageId);
        }
    }
}
