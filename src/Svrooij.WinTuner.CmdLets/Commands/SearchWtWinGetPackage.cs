using Svrooij.PowerShell.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace Svrooij.WinTuner.CmdLets.Commands;

/// <summary>
/// <para type="synopsis">Search for packages in winget</para>
/// <para type="description">Search for WinGet packages, but faster</para>
/// </summary>
/// <example>
/// <para type="description">Search for 'fire', did I tell you it's fast?</para>
/// <code>Search-WtWinGetPackage fire</code>
/// </example>
[Cmdlet(VerbsCommon.Search, "WtWinGetPackage", HelpUri = "https://wintuner.app/docs/wintuner-powershell/Search-WtWingetPackage")]
[OutputType(typeof(IReadOnlyCollection<Winget.CommunityRepository.Models.WingetEntry>))]
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

        foreach (var package in packages)
        {
            WriteObject(package);
        }
    }
}
