using Svrooij.PowerShell.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace Svrooij.WinTuner.CmdLets.Commands;

/// <summary>
/// <para type="synopsis">Search for packages in WinGet</para>
/// <para type="description">Search for WinGet packages, but faster. This uses the an [online index](https://wintuner.app/docs/related/winget-package-index).</para>
/// </summary>
/// <psOrder>9</psOrder>
/// <example>
/// <para type="name">Search for `fire`</para>
/// <para type="description">Searching in the online index, by a part of the package id.</para>
/// <code>Search-WtWinGetPackage fire</code>
/// </example>
[Cmdlet(VerbsCommon.Search, "WtWinGetPackage", HelpUri = "https://wintuner.app/docs/wintuner-powershell/Search-WtWingetPackage")]
[OutputType(typeof(Winget.CommunityRepository.Models.WingetEntry))]
public class SearchWtWinGetPackage : DependencyCmdlet<Startup>
{
    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 0,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "Part of the package ID, 2 characters minimum")]
    public string? PackageId { get; set; }

    [ServiceDependency]
    private Winget.CommunityRepository.WingetRepository wingetRepository;

    /// <inheritdoc />
    public override async Task ProcessRecordAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(PackageId) || PackageId.Length <= 1)
        {
            throw new ArgumentException("PackageId is required");
        }
        var packages = await wingetRepository.SearchPackage(PackageId ?? throw new ArgumentNullException(nameof(PackageId)), cancellationToken);

        WriteObject(packages, true);
    }
}
