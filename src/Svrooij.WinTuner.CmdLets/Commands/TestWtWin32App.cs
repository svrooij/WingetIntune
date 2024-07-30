using Microsoft.Extensions.Logging;
using Microsoft.Graph.Beta;
using Svrooij.PowerShell.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WingetIntune;
using WingetIntune.Graph;
using WingetIntune.Intune;
using WingetIntune.Models;
using WingetIntune.Testing;
using GraphModels = Microsoft.Graph.Beta.Models;

namespace Svrooij.WinTuner.CmdLets.Commands;
/// <summary>
/// <para type="synopsis">Test if a package will install</para>
/// <para type="description">Test if a package will install on the Windows Sandbox</para>
/// <para type="link" uri="https://wintuner.app/docs/wintuner-powershell/Test-WtWin32App">Documentation</para> 
/// </summary>
/// <example>
/// <para type="description">Test a packaged installer in sandbox</para>
/// <code>Test-WtWin32App -PackageFolder D:\packages\JanDeDobbeleer.OhMyPosh\22.0.3</code>
/// </example>
[Cmdlet(VerbsDiagnostic.Test, "WtWin32App", DefaultParameterSetName = nameof(PackageFolder))]
[OutputType(typeof(string))]
public class TestWtWin32App : DependencyCmdlet<Startup>
{
    private const string ParameterSetApp = "Win32LobApp";
    private const string ParameterSetWinGet = "WinGet";
    /// <summary>
    /// <para type="description">The Win32LobApp configuration you want to create</para>
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 0,
        ParameterSetName = ParameterSetApp,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "The App configuration you want to create")]
    public GraphModels.Win32LobApp? App { get; set; }

    /// <summary>
    /// <para type="description">The package id to upload to Intune.</para>
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 0,
        ParameterSetName = ParameterSetWinGet,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "The package id to upload to Intune.")]
    public string? PackageId { get; set; }

    /// <summary>
    /// <para type="description">The version to upload to Intune</para>
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 1,
        ParameterSetName = ParameterSetWinGet,
        ValueFromPipeline = false,
        HelpMessage = "The version to upload to Intune"
        )]
    public string? Version { get; set; }

    /// <summary>
    /// <para type="description">The Root folder where all the package live in.</para>
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 2,
        ParameterSetName = ParameterSetWinGet,
        ValueFromPipeline = false,
        HelpMessage = "The Root folder where all the package live in.")]
    public string? RootPackageFolder { get; set; }

    /// <summary>
    /// <para type="description">The folder where the package is</para>
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 0,
        ParameterSetName = nameof(PackageFolder),
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The folder where the package is")]
    public string? PackageFolder { get; set; }

    /// <summary>
    /// <para type="description">Clean the test files after run</para>
    /// </summary>
    [Parameter(
        Mandatory = false,
        HelpMessage = "Clean the test files after run")]
    public SwitchParameter Clean { get; set; }

    /// <summary>
    /// <para type="description">Sleep for x seconds before closing</para>
    /// </summary>
    [Parameter(
        Mandatory = false,
        HelpMessage = "Sleep for x seconds before auto shutdown")]
    public int? Sleep { get; set; }

    [ServiceDependency]
    private ILogger<TestWtWin32App>? logger;

    [ServiceDependency]
    private WindowsSandbox? sandbox;

    [ServiceDependency]
    private MetadataManager? metadataManager;

    /// <inheritdoc/>
    public override async Task ProcessRecordAsync(CancellationToken cancellationToken)
    {
        string? IntuneWinFile = null;
        PackageInfo? packageInfo = null;
        if (App is null)
        {
            if (ParameterSetName == ParameterSetWinGet)
            {
                logger?.LogDebug("Loading package details from RootPackageFolder {RootPackageFolder}, PackageId {PackageId}, Version {Version}", RootPackageFolder, PackageId, Version);
                PackageFolder = Path.Combine(RootPackageFolder!, PackageId!, Version!);
                logger?.LogDebug("Loading package details from folder {packageFolder}", PackageFolder);
            }

            if (PackageFolder is not null)
            {
                logger?.LogInformation("Loading package details from folder {packageFolder}", PackageFolder);
                packageInfo = await metadataManager!.LoadPackageInfoFromFolderAsync(PackageFolder, cancellationToken);
                App = metadataManager.ConvertPackageInfoToWin32App(packageInfo);
                IntuneWinFile = metadataManager.GetIntuneWinFileName(PackageFolder, packageInfo);
            }
            else
            {
                var ex = new ArgumentException("PackageFolder was provided");
                logger?.LogError(ex, "PackageFolder was provided");
                throw ex;
            }
        }



        var outputFolder = Path.Combine(Path.GetTempPath(), "wintuner-sandbox", Guid.NewGuid().ToString());

        var sandboxFile = await sandbox!.PrepareSandboxFileForPackage(packageInfo!, IntuneWinFile!, outputFolder, timeout: Sleep, cancellationToken: cancellationToken);
        logger?.LogDebug("Sandbox file created at {sandboxFile}", sandboxFile);
        var result = await sandbox.RunSandbox(sandboxFile, Clean, cancellationToken);
        if (result is null)
        {
            logger?.LogError("Sandbox exited with null result");
            return;
        }

        logger?.LogInformation("Installed {PackageId} {Version} in sandbox, reported exitcode {ExitCode}, number of apps installed {AppsInstalled}", packageInfo!.PackageIdentifier, packageInfo.Version, result.ExitCode, result?.InstalledApps?.Count());
        logger?.LogInformation("Sandbox result: {Result}", result);
        //if(Open)
        //{

        //    var result = await processManager.RunProcessAsync("WindowsSandbox.exe", sandboxFile, cancellationToken);
        //    logger?.LogInformation("Sandbox exited, wait a bit and cleanup");
        //    await Task.Delay(3000);
        //    Directory.Delete(outputFolder, true);
        //} else
        //{
        //    logger?.LogInformation("Sandbox file created at {sandboxFile}", sandboxFile);
        //    WriteObject(sandboxFile);
        //}
    }
}
