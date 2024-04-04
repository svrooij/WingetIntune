﻿using Microsoft.Extensions.Logging;
using Microsoft.Graph.Beta.Models.ODataErrors;
using Svrooij.PowerShell.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WingetIntune.Graph;

namespace Svrooij.WinTuner.CmdLets.Commands;

/// <summary>
/// <para type="synopsis">Remove an app from Intune</para>
/// <para type="description">Will remove the relationships (if any) first and then remove the app.</para>
/// <para type="link" uri="https://wintuner.app/docs/wintuner-powershell/Remove-WtWin32App">Documentation</para> 
/// </summary>
/// <example>
/// <para type="description">Delete a single app by ID with interactive authentication</para>
/// <code>Remove-WtWin32App -AppId "1450c17d-aee5-4bef-acf9-9e0107d340f2" -Username admin@myofficetenant.onmicrosoft.com</code>
/// </example>
[Cmdlet(VerbsCommon.Remove, "WtWin32App")]
public class RemoveWtWin32App : BaseIntuneCmdlet
{
    /// <summary>
    /// <para type="description">Id of the app in Intune</para>
    /// </summary>
    [Parameter(Mandatory = true,
        HelpMessage = "Id of the app in Intune")]
    public string? AppId { get; set; }

    [ServiceDependency]
    private ILogger<RemoveWtWin32App>? logger;

    [ServiceDependency]
    private HttpClient? httpClient;

    /// <inheritdoc/>
    public override async Task ProcessRecordAsync(CancellationToken cancellationToken)
    {
        ValidateAuthenticationParameters();
        logger?.LogInformation("Removing app {appId} from Intune", AppId);

        var graphServiceClient = CreateGraphServiceClient(httpClient!);

        // Load the app to get the relationships
        var app = await graphServiceClient.DeviceAppManagement.MobileApps[AppId].GetAsync(cancellationToken: cancellationToken);

        if (app?.SupersedingAppCount > 0) // This means deletion will fail
        {
            // Load the relationships to see if we can remove them
            var relationships = await graphServiceClient.DeviceAppManagement.MobileApps[AppId].Relationships.GetAsync(cancellationToken: cancellationToken);

            foreach (var relationship in relationships!.Value!.Where(r => r.TargetType == Microsoft.Graph.Beta.Models.MobileAppRelationshipType.Parent))
            {
                logger?.LogInformation("Updating relations of app {parentAppId} to remove {appId}", relationship.TargetId, AppId);
                var parentRelationShips = await graphServiceClient.DeviceAppManagement.MobileApps[relationship.TargetId].Relationships.GetAsync(cancellationToken: cancellationToken);
                await graphServiceClient.DeviceAppManagement.MobileApps[relationship.TargetId].UpdateRelationships.PostAsync(new Microsoft.Graph.Beta.DeviceAppManagement.MobileApps.Item.UpdateRelationships.UpdateRelationshipsPostRequestBody
                {
                    Relationships = parentRelationShips?.Value?.Where(r => r.TargetId != AppId).ToList() ?? new List<Microsoft.Graph.Beta.Models.MobileAppRelationship>()
                }, cancellationToken: cancellationToken);
            }

            logger?.LogInformation("Relationship removed, waiting 2 seconds before removing app");
            await Task.Delay(2000, cancellationToken);



            //try
            //{
            //    await graphServiceClient.DeviceAppManagement.MobileApps[AppId].DeleteAsync(cancellationToken: cancellationToken);
            //}
            //catch (ODataError ex)
            //{
            //    if (ex.Message.Contains("Cannot delete this app as it is the child of another app"))
            //    {
            //        string pattern = @"another app: (\b[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}\b)";
            //        var match = System.Text.RegularExpressions.Regex.Match(ex.Message, pattern);
            //        if (match.Success)
            //        {
            //            logger?.LogInformation("App {appId} is a child of {parentAppId}, removing relationships first", AppId, match.Groups[1].Value);

            //            // Load the parent app, to not break all relationships
            //            var parentApp = await graphServiceClient.DeviceAppManagement.MobileApps[match.Groups[1].Value].GetAsync(req =>
            //            {
            //                req.QueryParameters.Expand = new string[] { "relationships" };
            //            }, cancellationToken: cancellationToken);

            //            // Update the relations of the parent app, to remove this specific app
            //            await graphServiceClient.DeviceAppManagement.MobileApps[parentApp!.Id!].UpdateRelationships.PostAsync(new Microsoft.Graph.Beta.DeviceAppManagement.MobileApps.Item.UpdateRelationships.UpdateRelationshipsPostRequestBody
            //            {
            //                Relationships = parentApp.Relationships?.Where(r => r.TargetId != AppId).ToList() ?? new List<Microsoft.Graph.Beta.Models.MobileAppRelationship>()
            //            }, cancellationToken: cancellationToken);

            //            logger?.LogInformation("Relationship removed, waiting 2 seconds before retrying");
            //            await Task.Delay(2000, cancellationToken);
            //        }
            //        else
            //        {
            //            logger?.LogWarning(ex, "Failed to parse parent app id from error message");
            //            throw;
            //        }
            //    }
            //    else
            //    {
            //        throw;
            //    }


            //}
        }

        await graphServiceClient.DeviceAppManagement.MobileApps[AppId].DeleteAsync(cancellationToken: cancellationToken);
        logger?.LogInformation("App {appId} removed from Intune", AppId);
    }
}
