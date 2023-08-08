namespace WingetIntune;

public interface IProcessManager
{
    Task<ProcessResult> RunProcessAsync(string fileName, string arguments, CancellationToken cancellationToken = default, bool admin = false);
}

public record ProcessResult(int ExitCode, string Output, string Error);