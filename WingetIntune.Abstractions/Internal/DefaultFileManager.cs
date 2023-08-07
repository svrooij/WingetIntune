using Microsoft.Extensions.Logging;

namespace WingetIntune;

public partial class DefaultFileManager : IFileManager
{
    private readonly ILogger<DefaultFileManager> logger;

    public DefaultFileManager(ILogger<DefaultFileManager> logger)
    {
        this.logger = logger;
    }

    public void CreateFolder(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public string CreateFolderForPackage(string parentFolder, string packageName, string packageVersion)
    {
        LogCreatingFolder(packageName, packageVersion);
        string folder = Path.Combine(parentFolder, packageName, packageVersion);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        return folder;
    }

    public bool FileExists(string path)
    {
        LogFileExists(path);
        return File.Exists(path);
    }

    public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken)
    {
        LogWritingBytes(bytes.Length, path);
        return File.WriteAllBytesAsync(path, bytes, cancellationToken);
    }

    public Task WriteAllTextAsync(string path, string text, CancellationToken cancellationToken)
    {
        LogWritingText(path);
        return File.WriteAllTextAsync(path, text, cancellationToken);
    }

    [LoggerMessage(EventId = 100, Level = LogLevel.Debug, Message = "Creating folder for package {PackageName} {PackageVersion}")]
    private partial void LogCreatingFolder(string packageName, string packageVersion);

    [LoggerMessage(EventId = 101, Level = LogLevel.Debug, Message = "Checking if file exists: {Path}")]
    private partial void LogFileExists(string Path);

    [LoggerMessage(EventId = 102, Level = LogLevel.Debug, Message = "Writing {Bytes} bytes to {Path}")]
    private partial void LogWritingBytes(int Bytes, string Path);

    [LoggerMessage(EventId = 103, Level = LogLevel.Debug, Message = "Writing text to {Path}")]
    private partial void LogWritingText(string Path);
}