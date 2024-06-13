using Microsoft.Extensions.Logging;
using Svrooij.PowerShell.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WingetIntune.Graph;

namespace Svrooij.WinTuner.CmdLets.Commands;

/// <summary>
/// <para type="synopsis">Get all apps from Intune packaged by WinTuner</para>
/// <para type="description">Load apps from Tenant and filter based on Update Availabe, pipe to `New-IntuneWinPackage`</para>
/// <para type="link" uri="https://wintuner.app/docs/wintuner-powershell/Get-WtWin32Apps">Documentation</para> 
/// </summary>
/// <example>
/// <para type="description">Get all apps that have updates, using interactive authentication</para>
/// <code>Get-WtWin32Apps -Update $true -Username admin@myofficetenant.onmicrosoft.com</code>
/// </example>
[Cmdlet(VerbsCommon.Get, "WtWin32Apps")]
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
    public override async Task ProcessRecordAsync(CancellationToken cancellationToken)
    {
        ValidateAuthenticationParameters();
        logger?.LogInformation("Getting list of published apps");

        var graphServiceClient = gcf!.CreateClient(CreateAuthenticationProvider(cancellationToken: cancellationToken));
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

        foreach (var item in result)
        {
            WriteObject(item);
        }
    }
}
