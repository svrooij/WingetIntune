using Microsoft.Extensions.Logging;
using Svrooij.PowerShell.DependencyInjection;
using System;
using System.IO;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace Svrooij.WinTuner.CmdLets.Commands;

/// <summary>
/// <para type="synopsis">Create a new IntuneWin package</para>
/// <para type="description">This is a re-implementation of the IntuneWinAppUtil.exe tool, it's not feature complete use at your own risk.</para>
/// <para type="link" uri="https://wintuner.app/docs/related/content-prep-tool">Documentation</para>
/// </summary>
/// <example>
/// <para type="description">Package all files in C:\Temp\Source, with setup file ..\setup.exe to the specified folder</para>
/// <code>New-IntuneWinPackage -SourcePath C:\Temp\Source -SetupFile C:\Temp\Source\setup.exe -DestinationPath C:\Temp\Destination</code>
/// </example>
[Cmdlet(VerbsCommon.New, "IntuneWinPackage", HelpUri = "https://wintuner.app/docs/related/content-prep-tool#new-intunewinpackage")]
public class NewIntuneWinPackage : DependencyCmdlet<Startup>
{
    /// <summary>
    /// <para type="description">Directory containing all the installation files</para>
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 0,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The directory containing all the installation files")]
    public string? SourcePath { get; set; }

    /// <summary>
    /// <para type="description">The main setupfile in the source directory</para>
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 1,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The main setupfile in the source directory")]
    public string? SetupFile { get; set; }

    /// <summary>
    /// <para type="description">Destination folder</para>
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 2,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "Destination folder")]
    public string? DestinationPath { get; set; }


    [ServiceDependency]
    private ILogger<NewIntuneWinPackage>? _logger;

    [ServiceDependency]
    private SvRooij.ContentPrep.Packager? _packager;
    /// <inheritdoc/>
    public override async Task ProcessRecordAsync(CancellationToken cancellationToken)
    {
        try
        {
            var setupFile = Path.Combine(SourcePath!, SetupFile!);
            if (!Directory.Exists(DestinationPath!))
            {
                WriteVerbose($"Creating destination folder {DestinationPath}");
                Directory.CreateDirectory(DestinationPath!);
            }
            _logger?.LogInformation("Creating package for {setupFile}", setupFile);

            await _packager!.CreatePackage(SourcePath!, setupFile, DestinationPath!, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error creating a package");
        }
    }
}
