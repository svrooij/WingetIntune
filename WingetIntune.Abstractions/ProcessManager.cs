using Microsoft.Extensions.Logging;
using System.Text;

namespace WingetIntune;

public partial class ProcessManager : IProcessManager
{
    private readonly ILogger<ProcessManager> logger;

    public ProcessManager(ILogger<ProcessManager> logger)
    {
        this.logger = logger;
    }

    public async Task<ProcessResult> RunProcessAsync(string fileName, string arguments, CancellationToken cancellationToken = default, bool admin = false)//, string workingDirectory, bool waitForExit)
    {
        LogProcessStarting(fileName, arguments);
        using var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = fileName;
        process.StartInfo.Arguments = arguments;
        //process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.UseShellExecute = admin;
        process.StartInfo.Verb = admin ? "runas" : "";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                error.AppendLine(e.Data);
            }
        };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        var exitCode = process.ExitCode;
        LogProcessExited(exitCode, error.ToString());

        return new ProcessResult(exitCode, output.ToString(), error.ToString());
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Running {FileName} {Arguments}")]
    private partial void LogProcessStarting(string fileName, string arguments);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Process exited with code {ExitCode}.\n{Error}")]
    private partial void LogProcessExited(int ExitCode, string? Error);
}