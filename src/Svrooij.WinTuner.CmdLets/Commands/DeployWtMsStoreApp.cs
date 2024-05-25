using Microsoft.Extensions.Logging;
using Svrooij.PowerShell.DependencyInjection;
using System;
using System.Management.Automation;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WingetIntune.Graph;
using GraphModels = Microsoft.Graph.Beta.Models;

namespace Svrooij.WinTuner.CmdLets.Commands;
/// <summary>
/// <para type="synopsis">Create a MsStore app in Intune</para>
/// <para type="description">Use this command to create an Microsoft Store app in Microsoft Intune</para>
/// <para type="link" uri="https://wintuner.app/docs/wintuner-powershell/Deploy-WtMsStoreApp">Documentation</para> 
/// </summary>
/// <example>
/// <para type="description">Add Firefox to Intune, using interactive authentication</para>
/// <code>Deploy-WtMsStoreApp -PackageId 9NZVDKPMR9RD -Username admin@myofficetenant.onmicrosoft.com</code>
/// </example>
[Cmdlet(VerbsLifecycle.Deploy, "WtMsStoreApp", DefaultParameterSetName = nameof(PackageId))]
[OutputType(typeof(GraphModels.WinGetApp))]
public class DeployWtMsStoreApp : BaseIntuneCmdlet
{
    /// <summary>
    /// <para type="description">The package id to upload to Intune.</para>
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 0,
        ParameterSetName = nameof(PackageId),
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "The package id to upload to Intune.")]
    public string? PackageId { get; set; }

    /// <summary>
    /// <para type="description">Name of the app to look for, first match will be created.</para>
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 0,
        ParameterSetName = nameof(SearchQuery),
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Name of the app to look for, first match will be created.")]
    public string? SearchQuery { get; set; }

    [ServiceDependency]
    private ILogger<DeployWtMsStoreApp>? logger;

    [ServiceDependency]
    private GraphStoreAppUploader? graphStoreAppUploader;

    [ServiceDependency]
    private WingetIntune.Graph.GraphClientFactory? gcf;

    /// <inheritdoc/>
    public override async Task ProcessRecordAsync(CancellationToken cancellationToken)
    {
        ValidateAuthenticationParameters();
        if (ParameterSetName == nameof(SearchQuery))
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrWhiteSpace(SearchQuery);
#endif
            logger!.LogInformation("Searching package id for {searchQuery}", SearchQuery);
            PackageId = await graphStoreAppUploader!.GetStoreIdForNameAsync(SearchQuery!, cancellationToken);
            if (string.IsNullOrEmpty(PackageId))
            {
                logger!.LogError("No package found for {searchQuery}", SearchQuery);
                return;
            }
        }

        // At this moment the package ID should always be filled.

#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(PackageId);
#endif
        logger!.LogInformation("Uploading MSStore app {PackageId} to Intune", PackageId);
        var graphServiceClient = gcf!.CreateClient(CreateAuthenticationProvider(cancellationToken: cancellationToken));
        try
        {
            var app = await graphStoreAppUploader!.CreateStoreAppAsync(graphServiceClient, PackageId!, cancellationToken);

            logger!.LogInformation("Created MSStore app {PackageId} with id {appId}", PackageId, app!.Id);
            WriteObject(app);
        }
        catch (Exception ex)
        {
            logger!.LogError(ex, "Error creating MSStore app {PackageId}", PackageId);
        }


    }
}
