using Microsoft.Extensions.Logging;
using Svrooij.PowerShell.DependencyInjection;
using System;
using System.IO;
using System.Management.Automation;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WingetIntune.Graph;
using WingetIntune.Intune;
using GraphModels = Microsoft.Graph.Beta.Models;

namespace Svrooij.WinTuner.CmdLets.Commands;
/// <summary>
/// <para type="synopsis">Create a Win32Lob app in Intune</para>
/// <para type="description">Use this command to upload an intunewin package to Microsoft Intune as a new Win32LobApp.</para>
/// </summary>
/// <example>
/// <para type="description">Upload a pre-packaged application, from just it's folder, using interactive authentication</para>
/// <code>Deploy-WtWin32App -PackageFolder C:\Tools\packages\JanDeDobbeleer.OhMyPosh\19.5.2 -Username admin@myofficetenant.onmicrosoft.com</code>
/// </example>
[Cmdlet(VerbsLifecycle.Deploy, "WtWin32App", DefaultParameterSetName = ParameterSetApp)]
[OutputType(typeof(GraphModels.Win32LobApp))]
public class DeployWtWin32App : BaseIntuneCmdlet
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
    /// <para type="description">The .intunewin file that should be added to this app</para>
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 1,
        ParameterSetName = ParameterSetApp,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "The .intunewin file that should be added to this app")]
    public string? IntuneWinFile { get; set; }

    /// <summary>
    /// <para type="description">Load the logo from file (optional)</para>
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 2,
        ParameterSetName = ParameterSetApp,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Load the logo from file")]
    public string? LogoPath { get; set; }

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

    [ServiceDependency]
    private ILogger<DeployWtWin32App>? logger;

    [ServiceDependency]
    private GraphAppUploader? graphAppUploader;

    [ServiceDependency]
    private MetadataManager? metadataManager;

    [ServiceDependency]
    private HttpClient? httpClient;

    /// <inheritdoc/>
    public override async Task ProcessRecordAsync(CancellationToken cancellationToken)
    {
        ValidateAuthenticationParameters();

        if (App is null)
        {
            if (ParameterSetName == ParameterSetWinGet)
            {
                PackageFolder = Path.Combine(RootPackageFolder!, PackageId!, Version!);
            }

            if (PackageFolder is not null)
            {
                logger?.LogInformation("Loading package details from folder {packageFolder}", PackageFolder);
                var packageInfo = await metadataManager!.LoadPackageInfoFromFolderAsync(PackageFolder, cancellationToken);
                App = metadataManager.ConvertPackageInfoToWin32App(packageInfo);
                LogoPath = Path.Combine(PackageFolder, "..", "logo.png");
                IntuneWinFile = metadataManager.GetIntuneWinFileName(PackageFolder, packageInfo);
            }
            else
            {
                throw new ArgumentException("No package or package id specified");
            }
        }

        logger?.LogInformation("Uploading Win32App {DisplayName} to Intune with file {IntuneWinFile}", App!.DisplayName, IntuneWinFile);
        var graphServiceClient = CreateGraphServiceClient(httpClient!);
        var newApp = await graphAppUploader!.CreateNewAppAsync(graphServiceClient, App, IntuneWinFile!, LogoPath, cancellationToken);
        logger?.LogInformation("Created Win32App {DisplayName} with id {Id}", newApp!.DisplayName, newApp.Id);
        WriteObject(newApp!);
    }
}
