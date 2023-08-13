using Microsoft.Extensions.Logging;

namespace WingetIntune;

public partial class DefaultFileManager : IFileManager
{
    private readonly ILogger<DefaultFileManager> logger;
    private readonly HttpClient httpClient;

    public DefaultFileManager(ILogger<DefaultFileManager> logger, HttpClient? httpClient)
    {
        this.logger = logger;
        this.httpClient = httpClient ?? new HttpClient();
    }

    public void CopyFile(string sourcePath, string destinationPath, bool overwrite = false)
    {
        var destinationFolder = Path.GetDirectoryName(destinationPath);
        if (!Directory.Exists(destinationFolder))
            Directory.CreateDirectory(destinationFolder!);
        File.Copy(sourcePath, destinationPath, overwrite);
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

    public void DeleteFileOrFolder(string path)
    {
        if(Path.Exists(path))
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
            else
                File.Delete(path);
        }
    }

    public async Task DownloadFileAsync(string url, string path, bool overrideFile = false, CancellationToken cancellationToken = default)
    {
        if(overrideFile || !File.Exists(path))
        {
            logger.LogInformation("Downloading {url} to {path}", url, path);
            var result = await httpClient.GetAsync(url, cancellationToken);
            result.EnsureSuccessStatusCode();
            var data = await result.Content.ReadAsByteArrayAsync(cancellationToken);
            await File.WriteAllBytesAsync(path, data, cancellationToken);
        }
        else
        {
            logger.LogInformation("Skipping download of {url} to {path} because the file already exists", url, path);
        }
    }

    public void ExtractFileToFolder(string zipPath, string folderPath)
    {
        System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, folderPath);
    }

    public bool FileExists(string path)
    {
        LogFileExists(path);
        return File.Exists(path);
    }

    public string FindFile(string folder, string filename)
    {
        // Recursursively search for the file in the folder
        foreach (var file in Directory.GetFiles(folder, filename, SearchOption.AllDirectories))
        {
            if (Path.GetFileName(file).Equals(filename, StringComparison.OrdinalIgnoreCase))
                return file;
        }

        throw new FileNotFoundException($"Could not find file {filename} in folder {folder}");
    }

    public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken)
    {
        return File.ReadAllBytesAsync(path, cancellationToken);
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