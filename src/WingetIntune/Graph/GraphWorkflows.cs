using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using WingetIntune.Extensions;
using WingetIntune.Intune;

namespace WingetIntune.Graph;

public static class GraphWorkflows
{
    public static async Task AddIntuneCategoriesToApp(this GraphServiceClient graphServiceClient, string appId, string[] categories, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(graphServiceClient);
        ArgumentException.ThrowIfNullOrEmpty(appId);
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(cancellationToken);

        // Load categories to match against
        var graphCategories = await graphServiceClient.DeviceAppManagement.MobileAppCategories.GetAsync(cancellationToken: cancellationToken);

        // Match categories by name and add to app
        var foundCategories = categories.Select(c => graphCategories!.Value!.SingleOrDefault(x => x.DisplayName?.Equals(c, StringComparison.InvariantCultureIgnoreCase) == true)?.Id).Where(c => !string.IsNullOrEmpty(c)).ToArray();
        var batch = new Microsoft.Graph.BatchRequestContentCollection(graphServiceClient);
        foreach (var categoryId in foundCategories)
        {
            await batch.AddBatchRequestStepAsync(graphServiceClient.Intune_AddCategoryToApp_RequestInfo(appId, categoryId!));
        }
        await graphServiceClient.Batch.PostAsync(batch, cancellationToken);
    }

    public static async Task<int> AssignAppAsync(this GraphServiceClient graphServiceClient, string appId, string[]? requiredFor, string[]? availableFor, string[]? uninstallFor, CancellationToken cancellationToken)
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
