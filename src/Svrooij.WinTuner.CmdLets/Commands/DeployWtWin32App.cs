using Microsoft.Extensions.Logging;
using Microsoft.Graph.Beta;
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
/// <para type="link" uri="https://wintuner.app/docs/wintuner-powershell/Deploy-WtWin32App">Documentation</para> 
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

    /// <summary>
    /// Override the name of the app in Intune
    /// </summary>
    [Parameter(HelpMessage = "Override the name of the app in Intune", Mandatory = false)]
    public string? OverrideAppName { get; set; }

    /// <summary>
    /// <para type="description">The graph id of the app to supersede</para>
    /// </summary>
    [Parameter(DontShow = true, HelpMessage = "Graph ID of the app to supersede", Mandatory = false)]
    public string? GraphId { get; set; }

    [ServiceDependency]
    private ILogger<DeployWtWin32App>? logger;

    [ServiceDependency]
    private GraphAppUploader? graphAppUploader;

    [ServiceDependency]
    private MetadataManager? metadataManager;

    [ServiceDependency]
    private WingetIntune.Graph.GraphClientFactory? gcf;

    /// <inheritdoc/>
    public override async Task ProcessRecordAsync(CancellationToken cancellationToken)
    {
        logger?.LogDebug("Validating authentication parameters");
        ValidateAuthenticationParameters();
        logger?.LogDebug("Authentication parameters validated");

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
                var packageInfo = await metadataManager!.LoadPackageInfoFromFolderAsync(PackageFolder, cancellationToken);
                App = metadataManager.ConvertPackageInfoToWin32App(packageInfo);
                LogoPath = Path.Combine(PackageFolder, "..", "logo.png");
                IntuneWinFile = metadataManager.GetIntuneWinFileName(PackageFolder, packageInfo);
            }
            else
            {
                var ex = new ArgumentException("No App or PackageFolder was provided");
                logger?.LogError(ex, "No App or PackageFolder was provided");
                throw ex;
            }
        }

        if (!string.IsNullOrEmpty(OverrideAppName))
        {
            App.DisplayName = OverrideAppName;
        }

        logger?.LogInformation("Uploading Win32App {DisplayName} to Intune with file {IntuneWinFile}", App!.DisplayName, IntuneWinFile);
        var graphServiceClient = gcf!.CreateClient(CreateAuthenticationProvider(cancellationToken: cancellationToken));
        var newApp = await graphAppUploader!.CreateNewAppAsync(graphServiceClient, App, IntuneWinFile!, LogoPath, cancellationToken);
        logger?.LogInformation("Created Win32App {DisplayName} with id {appId}", newApp!.DisplayName, newApp.Id);

        // Check if we need to supersede an app
        if (GraphId is not null)
        {
            await SupersedeApp(logger!, graphServiceClient, newApp!.Id!, GraphId, cancellationToken);
        }

        WriteObject(newApp!);
    }

    /// <summary>
    /// Supersede an app.
    /// </summary>
    /// <remarks>
    /// 1. Load the old app
    /// 2. Update relationships of the new app to supersede the old app
    /// 3. Copy categories from the old app to the new app
    /// 4. Copy assignments from the old app to the new app
    /// 5. Remove assignments from the old app
    /// </remarks>
    /// <param name="logger"></param>
    /// <param name="graphServiceClient"></param>
    /// <param name="newAppId"></param>
    /// <param name="oldAppId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task SupersedeApp(ILogger logger, GraphServiceClient graphServiceClient, string newAppId, string oldAppId, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Loading old app {oldAppId} to superseed", oldAppId);
        var oldApp = await graphServiceClient.DeviceAppManagement.MobileApps[oldAppId].GetAsync(req =>
        {
            req.QueryParameters.Expand = new string[] { "categories", "assignments" };
        }, cancellationToken);

        if (oldApp is GraphModels.Win32LobApp oldWin32App)
        {
            logger?.LogInformation("Superseeding app {oldAppId} with {appId}", oldAppId, newAppId);
            var batch = new Microsoft.Graph.BatchRequestContentCollection(graphServiceClient);
            // Add supersedence relationship to new app
            await batch.AddBatchRequestStepAsync(graphServiceClient.DeviceAppManagement.MobileApps[newAppId].UpdateRelationships.ToPostRequestInformation(new Microsoft.Graph.Beta.DeviceAppManagement.MobileApps.Item.UpdateRelationships.UpdateRelationshipsPostRequestBody
            {
                Relationships = new()
                    {
                        new GraphModels.MobileAppSupersedence
                        {
                            // TODO Should the SupersedenceType be Update or Replace, maybe configureable?
                            SupersedenceType = GraphModels.MobileAppSupersedenceType.Update,
                            TargetId = oldAppId!
                        }
                    }
            }));

            // Copy categories from old app to new app
            if (oldWin32App.Categories is not null && oldWin32App.Categories.Count > 0)
            {
                foreach (var c in oldWin32App.Categories)
                {
                    await batch.AddBatchRequestStepAsync(graphServiceClient.Intune_AddCategoryToApp_RequestInfo(newAppId, c.Id!));
                }
            }

            // Copy assignments from old app to new app
            if (oldWin32App.Assignments is not null && oldWin32App.Assignments.Count > 0)
            {
                // This part is to enable auto update for the new app, if that was not set on the old app
                var assignments = oldWin32App.Assignments;
                foreach (var assignment in assignments)
                {
                    if (assignment.Intent == GraphModels.InstallIntent.Available && assignment.Settings is null)
                    {
                        assignment.Settings = new GraphModels.Win32LobAppAssignmentSettings { Notifications = GraphModels.Win32LobAppNotification.ShowReboot };
                        assignment.Settings.AdditionalData.Add("autoUpdateSettings", new Win32LobAppAutoUpdateSettings());
                    }
                }

                await batch.AddBatchRequestStepAsync(graphServiceClient.DeviceAppManagement.MobileApps[newAppId].Assign.ToPostRequestInformation(new Microsoft.Graph.Beta.DeviceAppManagement.MobileApps.Item.Assign.AssignPostRequestBody
                {
                    MobileAppAssignments = assignments, //oldWin32App.Assignments
                }));

                // Remove assignments from old app
                await batch.AddBatchRequestStepAsync(graphServiceClient.DeviceAppManagement.MobileApps[oldAppId].Assign.ToPostRequestInformation(new Microsoft.Graph.Beta.DeviceAppManagement.MobileApps.Item.Assign.AssignPostRequestBody
                {
                    MobileAppAssignments = new System.Collections.Generic.List<GraphModels.MobileAppAssignment>()
                }));
            }

            // Execute batch
            await graphServiceClient.Batch.PostAsync(batch, cancellationToken);
        }
    }
}
