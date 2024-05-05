using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace WingetIntune.Implementations;

internal class AzCopyAzureUploader : IAzureFileUploader
{
    private readonly string azCopyPath;
    private readonly ILogger<AzCopyAzureUploader> logger;
    private readonly IProcessManager processManager;
    private readonly IFileManager fileManager;

    public AzCopyAzureUploader(ILogger<AzCopyAzureUploader>? logger, IProcessManager processManager, IFileManager fileManager)
    {
        ArgumentNullException.ThrowIfNull(processManager);
        ArgumentNullException.ThrowIfNull(fileManager);
        this.logger = logger ?? new NullLogger<AzCopyAzureUploader>();
        this.processManager = processManager;
        this.fileManager = fileManager;
        azCopyPath = Path.Combine(Path.GetTempPath(), "intunewin", "azcopy.exe");
    }

    private async Task DownloadAzCopyIfNeeded(CancellationToken cancellationToken)
    {
        if (!fileManager.FileExists(azCopyPath))
        {
            logger.LogInformation("Downloading AzCopy to {azCopyPath}", azCopyPath);
            var azCopyDownloadUrl = "https://aka.ms/downloadazcopy-v10-windows";
            var downloadPath = Path.GetTempFileName();
            await fileManager.DownloadFileAsync(azCopyDownloadUrl, downloadPath, throwOnFailure: true, overrideFile: true, cancellationToken: cancellationToken);

            var extractFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            logger.LogInformation("Extracting AzCopy to {path}", extractFolder);
            fileManager.ExtractFileToFolder(downloadPath, extractFolder);
            var azCopyExe = fileManager.FindFile(extractFolder, "azcopy.exe");
            fileManager.CopyFile(azCopyExe, azCopyPath);
            fileManager.DeleteFileOrFolder(extractFolder);
            fileManager.DeleteFileOrFolder(downloadPath);
        }
    }

    public async Task UploadFileToAzureAsync(string filename, Uri sasUri, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(filename);
        ArgumentNullException.ThrowIfNull(sasUri);
#endif
        await DownloadAzCopyIfNeeded(cancellationToken);
        var args = $"copy \"{filename}\" \"{sasUri}\" --output-type \"json\"";
        var result = await processManager.RunProcessAsync(azCopyPath, args, cancellationToken, false);

        logger.LogDebug("AzCopy result: {result}", result);
        if (result.ExitCode != 0)
        {
            var exception = new Exception($"AzCopy resulted in a non-zero exitcode.");
            exception.Data.Add("ExitCode", result.ExitCode);
            exception.Data.Add("Output", result.Output);
            exception.Data.Add("Error", result.Error);
            logger.LogWarning(exception, "AzCopy resulted in a non-zero exitcode.");
            throw exception;
        }

        logger.LogInformation("AzCopy upload {filename} to {sasUri} successfully", filename, sasUri);
    }
}
