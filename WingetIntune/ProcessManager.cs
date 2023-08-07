using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetIntune;

internal class ProcessManager
{
    public static async Task<ProcessResult> RunProcessAsync(string fileName, string arguments, CancellationToken cancellationToken = default, bool admin = false)//, string workingDirectory, bool waitForExit)
    {
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

        //if (waitForExit)
        //{
        //    process.WaitForExit();
        //}
        await process.WaitForExitAsync(cancellationToken);

        var exitCode = process.ExitCode;

        //if (exitCode != 0)
        //{
        //    throw new Exception($"Process exited with code {exitCode}.\n{error}");
        //}

        return new ProcessResult(exitCode, output.ToString(), error.ToString());
    }
}
internal record ProcessResult(int ExitCode, string Output, string Error);
