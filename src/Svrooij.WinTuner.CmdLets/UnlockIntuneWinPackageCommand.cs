using Microsoft.Extensions.Logging;
using Svrooij.PowerShell.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Svrooij.WinTuner.CmdLets;
/// <summary>
/// <para type="synopsis">Decrypt an IntuneWin package</para>
/// <para type="description">Decrypt IntuneWin files, based on this post https://svrooij.io/2023/10/09/decrypting-intunewin-files/</para>
/// <para type="link" uri="https://wintuner.app/docs/related/content-prep-tool#unlock-intunewinpackage">Documentation</para>
/// </summary>
/// <example>
/// <code>
/// Unlock-IntuneWinPackage -SourceFile "C:\Temp\Source\MyApp.intunewin" -DestinationPath "C:\Temp\Destination"
/// </code>
/// </example>
[Cmdlet(VerbsCommon.Unlock, "IntuneWinPackage", HelpUri = "https://wintuner.app/docs/related/content-prep-tool#unlock-intunewinpackage")]
public class UnlockIntuneWinPackageCommand : DependencyCmdlet<Startup>
{
    /// <summary>
    /// <para type="description">The location of the .intunewin file</para>
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 0,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "The location of the .intunewin file")]
    public string SourceFile { get; set; }

    /// <summary>
    /// <para type="description">Destination folder</para>
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 1,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "Destination folder")]
    public string DestinationPath { get; set; }

    [ServiceDependency]
    private SvRooij.ContentPrep.Packager packager;

    [ServiceDependency]
    private ILogger<UnlockIntuneWinPackageCommand> logger;

    public override async Task ProcessRecordAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(SourceFile))
            {
                WriteError(new ErrorRecord(new FileNotFoundException("File not found", SourceFile), "1", ErrorCategory.InvalidOperation, null));
                return;
            }
            if (!Directory.Exists(DestinationPath))
            {
                WriteVerbose($"Creating destination folder {DestinationPath}");
                Directory.CreateDirectory(DestinationPath);
            }

            await packager.Unpack(SourceFile, DestinationPath, cancellationToken);
        } catch (Exception ex)
        {
            logger.LogError(ex, "Error unlocking package");
        }
    }
}
