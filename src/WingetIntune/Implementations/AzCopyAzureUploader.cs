using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetIntune;
internal class AzCopyAzureUploader : IAzureFileUploader
{
    private readonly string azCopyPath;
    private readonly ILogger<AzCopyAzureUploader> logger;
    private readonly IProcessManager processManager;
    private readonly IFileManager fileManager;

    public AzCopyAzureUploader(ILogger<AzCopyAzureUploader> logger, IProcessManager processManager, IFileManager fileManager)
    {
        azCopyPath = Path.Combine(Path.GetTempPath(), "intunewin", "azcopy.exe");
        this.logger = logger;
        this.processManager = processManager;
        this.fileManager = fileManager;
    }

    private async Task DownloadAzCopyIfNeeded(CancellationToken cancellationToken)
    {
        if(!fileManager.FileExists(azCopyPath))
        {
            logger.LogInformation("Downloading AzCopy to {azCopyPath}", azCopyPath);
            var azCopyDownloadUrl = "https://aka.ms/downloadazcopy-v10-windows";
            var downloadPath = Path.GetTempFileName();
            await fileManager.DownloadFileAsync(azCopyDownloadUrl, downloadPath, overrideFile: true, cancellationToken);

            
            var extractFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            logger.LogInformation("Extracting AzCopy to {path}", extractFolder);
            fileManager.ExtractFileToFolder(downloadPath, extractFolder);
            var azCopyExe = fileManager.FindFile(extractFolder, "azcopy.exe");
            fileManager.CopyFile(azCopyExe, azCopyPath);
            fileManager.DeleteFileOrFolder(extractFolder);
            fileManager.DeleteFileOrFolder(downloadPath);
        }
    }

    public async Task UploadFileToAzureAsync(string filename, Uri sasUri, CancellationToken cancellationToken)
    {
        await DownloadAzCopyIfNeeded(cancellationToken);
        var args = $"copy \"{filename}\" \"{sasUri}\" --output-type \"json\"";
        var result = await processManager.RunProcessAsync(azCopyPath, args, cancellationToken);
        logger.LogInformation("AzCopy result: {result}", result);
        if (result.ExitCode != 0)
        {
            var exception = new Exception($"AzCopy resulted in a non-zero exitcode.");
            exception.Data.Add("ExitCode", result.ExitCode);
            exception.Data.Add("Output", result.Output);
            exception.Data.Add("Error", result.Error);
            logger.LogWarning(exception, "AzCopy resulted in a non-zero exitcode.");
            throw exception;
        }
    }
}
