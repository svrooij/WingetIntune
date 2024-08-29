using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions.Authentication;
using Svrooij.PowerShell.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using WingetIntune.Graph;

namespace Svrooij.WinTuner.CmdLets.Commands;

/// <summary>
/// <para type="synopsis">Get all apps from Intune packaged by WinTuner</para>
/// <para type="description">Load apps from Tenant and filter based on Update available.\r\n\r\nThis is an [**authenticated command**](./authentication), so call [Connect-WtWinTuner](./Connect-WtWinTuner) before calling this command.</para>
/// </summary>
/// <psOrder>12</psOrder>
/// <example>
/// <para type="name">Get all apps with updates</para>
/// <para type="description">Get all apps that have updates available</para>
/// <code>Get-WtWin32Apps -Update $true</code>
/// </example>
/// <example>
/// <para type="name">Update apps</para>
/// <para type="description">Get all apps that have be an update available and are not superseeded. This executes the [New-WtWingetPackage](./New-WtWingetPackage) command.\r\nYou could run this on a weekly bases.</para>
/// <code>$updatedApps = Get-WtWin32Apps -Update $true -Superseded $false\r\nforeach($app in $updatedApps) { New-WtWingetPackage -PackageId $($app.PackageId) -PackageFolder $rootPackageFolder -Version $($app.LatestVersion) | Deploy-WtWin32App -GraphId $($app.GraphId) }</code>
/// </example>
/// <example>
/// <para type="name">Remove superseeded apps</para>
/// <para type="description">Get all apps that have been superseeded and remove them. This executes the [Remove-WtWin32App](./Remove-WtWin32App) command.\r\nYou could run this on a weekly bases.</para>
/// <code>$oldApps = Get-WtWin32Apps -Superseded $true\r\nforeach($app in $oldApps) { Remove-WtWin32App -AppId $app.GraphId }</code>
/// </example>
[Cmdlet(VerbsCommon.Get, "WtWin32Apps", HelpUri = "https://wintuner.app/docs/wintuner-powershell/Get-WtWin32Apps")]
[OutputType(typeof(Models.WtWin32App[]))]
public class GetWtWin32Apps : BaseIntuneCmdlet
{
    /// <summary>
    /// <para type="description">Filter based on UpdateAvailable</para>
    /// </summary>
    [Parameter(Mandatory = false,
        HelpMessage = "Filter based on UpdateAvailable")]
    public bool? Update { get; set; }

    /// <summary>
    /// <para type="description">Filter based on SupersedingAppCount</para>
    /// </summary>
    [Parameter(Mandatory = false,
               HelpMessage = "Filter based on SupersedingAppCount")]
    public bool? Superseded { get; set; }

    /// <summary>
    /// <para type="description">Filter based on SupersedingAppCount</para>
    /// </summary>
    [Parameter(Mandatory = false,
                      HelpMessage = "Filter based on SupersedingAppCount")]
    public bool? Superseding { get; set; }

    [ServiceDependency]
    private ILogger<GetWtWin32Apps>? logger;

    [ServiceDependency]
    private WingetIntune.Graph.GraphClientFactory? gcf;

    [ServiceDependency]
    private Winget.CommunityRepository.WingetRepository? repo;

    /// <inheritdoc/>
    protected override async Task ProcessAuthenticatedAsync(IAuthenticationProvider provider, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Getting list of published apps");

        var graphServiceClient = gcf!.CreateClient(provider);
        var apps = await graphServiceClient.DeviceAppManagement.MobileApps.GetWinTunerAppsAsync(cancellationToken);

        List<Models.WtWin32App> result = new();

        foreach (var app in apps)
        {
            var version = await repo!.GetLatestVersion(app.PackageId, cancellationToken);
            result.Add(new Models.WtWin32App
            {
                GraphId = app.GraphId,
                PackageId = app.PackageId,
                Name = app.Name,
                CurrentVersion = app.CurrentVersion,
                SupersededAppCount = app.SupersededAppCount,
                SupersedingAppCount = app.SupersedingAppCount,
                InstallerContext = app.InstallerContext,
                Architecture = app.Architecture,
                LatestVersion = version,
            });
        }

        if (Update.HasValue)
        {
            result = result.Where(x => x.IsUpdateAvailable == Update.Value).ToList();
        }

        if (Superseded.HasValue)
        {
            result = result.Where(x => x.SupersedingAppCount > 0 == Superseded.Value).ToList();
        }

        if (Superseding.HasValue)
        {
            result = Superseding.Value ? result.Where(x => x.SupersededAppCount > 0).ToList() : result.Where(x => x.SupersededAppCount == 0).ToList();
        }

        WriteObject(result, true);
    }
}
