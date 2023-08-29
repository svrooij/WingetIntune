using Microsoft.Extensions.Logging;
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingetIntune.Extensions;
using WingetIntune.Intune;

namespace WingetIntune.Graph;
public static class GraphWorkflows
{
    public static async Task AddIntuneCategoriesToApp(GraphServiceClient graphServiceClient, string appId, string[] categories, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(graphServiceClient);
        ArgumentException.ThrowIfNullOrEmpty(appId);
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(cancellationToken);

        // Load categories to match against
        var graphCategories = await graphServiceClient.DeviceAppManagement.MobileAppCategories.GetAsync(cancellationToken: cancellationToken);

        // Match categories by name and add to app
        var foundCategories = categories.Select(c => graphCategories!.Value!.SingleOrDefault(x => x.DisplayName?.Equals(c, StringComparison.InvariantCultureIgnoreCase) == true)?.Id).ToArray();
        foreach (var categoryId in foundCategories)
        {
            if (!string.IsNullOrEmpty(categoryId))
            {
                await graphServiceClient.Intune_AddCategoryToApp(appId, categoryId, cancellationToken);
            }
        }
    }

    public static async Task<int> AssignAppAsync(GraphServiceClient graphServiceClient, string appId, string[]? requiredFor, string[]? availableFor, string[]? uninstallFor, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(graphServiceClient);
        ArgumentException.ThrowIfNullOrEmpty(appId);
        ArgumentNullException.ThrowIfNull(cancellationToken);

        List<MobileAppAssignment> assignments = new List<MobileAppAssignment>();
        if (requiredFor is not null && requiredFor.Any())
        {
            assignments.AddRange(GenerateAssignments(requiredFor, InstallIntent.Required));
        }

        if (availableFor is not null && availableFor.Any())
        {
            assignments.AddRange(GenerateAssignments(availableFor, InstallIntent.Available));
        }

        if (uninstallFor is not null && uninstallFor.Any())
        {
            assignments.AddRange(GenerateAssignments(uninstallFor, InstallIntent.Uninstall));
        }

        if (assignments.Count == 0)
        {
            return -1;
        }


        await graphServiceClient.DeviceAppManagement.MobileApps[appId].Assign.PostAsync(new Microsoft.Graph.Beta.DeviceAppManagement.MobileApps.Item.Assign.AssignPostRequestBody
        {
            MobileAppAssignments = assignments
        }, cancellationToken: cancellationToken);
        return assignments.Count;

    }

    private static List<MobileAppAssignment> GenerateAssignments(string[] groups, InstallIntent intent)
    {
        List<MobileAppAssignment> assignments = new List<MobileAppAssignment>();
        if (groups is not null && groups.Any())
        {
            var groupsGuids = groups.Where(x => Guid.TryParse(x, out _));
            if (groupsGuids.Count() > 0)
            {
                assignments.AddRange(groupsGuids.Select(x => new MobileAppAssignment
                {
                    Intent = intent,
                    Target = new GroupAssignmentTarget
                    {
                        GroupId = x
                    }
                }));
            }
            if (groups.ContainsIgnoreCase(IntuneManagerConstants.AllUsers))
            {
                assignments.Add(new MobileAppAssignment
                {
                    Intent = intent,
                    Target = new AllLicensedUsersAssignmentTarget()
                });
            }
            if (groups.ContainsIgnoreCase(IntuneManagerConstants.AllDevices))
            {
                assignments.Add(new MobileAppAssignment
                {
                    Intent = intent,
                    Target = new AllDevicesAssignmentTarget()
                });
            }
        }
        return assignments;
    }
}
