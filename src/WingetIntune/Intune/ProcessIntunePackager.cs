using Microsoft.Extensions.Logging;
using WingetIntune.Interfaces;
using WingetIntune.Models;

namespace WingetIntune.Intune;

/// <summary>
/// Process based implementation of <see cref="IIntunePackager"/>. This implementation uses the IntuneWinAppUtil.exe tool to create the .intunewin package.
/// </summary>
/// <remarks>
/// Will be removed when there is a proper API to create .intunewin packages. Maybe use https://github.com/volodymyrsmirnov/IntuneWin
/// </remarks>
public partial class ProcessIntunePackager : IIntunePackager
{
    internal const string IntuneWinAppUtil = "IntuneWinAppUtil.exe";
    internal const string IntuneWinAppUtilUrl = "https://github.com/microsoft/Microsoft-Win32-Content-Prep-Tool/raw/master/IntuneWinAppUtil.exe";

    private readonly string toolPath = Path.Combine(Path.GetTempPath(), "Intune", IntuneWinAppUtil);
    private readonly IProcessManager processManager;
    private readonly IFileManager fileManager;
    private readonly ILogger<ProcessIntunePackager> logger;

    public ProcessIntunePackager(IProcessManager processManager, IFileManager fileManager, ILogger<ProcessIntunePackager> logger)
    {
        this.processManager = processManager;
        this.fileManager = fileManager;
        this.logger = logger;
    }

    public Task CreatePackage(string inputFolder, string outputFolder, string installerFilename, CancellationToken cancellationToken) => CreatePackage(inputFolder, outputFolder, installerFilename, null, cancellationToken);

    public async Task<string> CreatePackage(string inputFolder, string outputFolder, string installerFilename, PackageInfo? _ = null, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(inputFolder);
        ArgumentException.ThrowIfNullOrEmpty(outputFolder);
        ArgumentException.ThrowIfNullOrEmpty(installerFilename);
        ArgumentNullException.ThrowIfNull(cancellationToken);
#endif
        await DownloadToolIfNeeded(cancellationToken);

        LogCreatePackage(inputFolder, outputFolder);
        var args = $"-c {inputFolder} -s {installerFilename} -o {outputFolder} -q";

        var result = await processManager.RunProcessAsync(toolPath, args, cancellationToken);

        if (result.ExitCode != 0)
        {
            var exception = new Exception($"Generating .intunewin resulted in a non-zero exitcode.");
            exception.Data.Add("ExitCode", result.ExitCode);
            exception.Data.Add("Output", result.Output);
            exception.Data.Add("Error", result.Error);
            LogWarning(exception);
            throw exception;
        }

        return Path.GetFileNameWithoutExtension(installerFilename) + ".intunewin";
    }

    private Task DownloadToolIfNeeded(CancellationToken cancellationToken)
    {
        if (!fileManager.FileExists(toolPath))
        {
            logger.LogInformation("Downloading {tool} to {path}", IntuneWinAppUtil, toolPath);
            return fileManager.DownloadFileAsync(IntuneWinAppUtilUrl, toolPath, cancellationToken: cancellationToken);
        }
        return Task.CompletedTask;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Creating Intune package from {inputFolder} to {outputFolder}")]
    private partial void LogCreatePackage(string inputFolder, string outputFolder);

    //logger.LogWarning(exception, "Generating .intunewin resulted in a non-zero exitcode.");
    [LoggerMessage(EventId = 100, Level = LogLevel.Warning, Message = "Generating .intunewin resulted in a non-zero exitcode.")]
    private partial void LogWarning(Exception exception);
}
