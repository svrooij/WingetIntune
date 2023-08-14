namespace WingetIntune;

public interface IFileManager
{
    void CopyFile(string sourcePath, string destinationPath, bool overwrite = false);
    void CreateFolder(string path);

    string CreateFolderForPackage(string parentFolder, string packageName, string packageVersion);

    void DeleteFileOrFolder(string path);

    Task DownloadFileAsync(string url, string path, bool throwOnFailure = true, bool overrideFile = false, CancellationToken cancellationToken = default);

    void ExtractFileToFolder(string zipPath, string folderPath);

    string FindFile(string folder, string filename);

    bool FileExists(string path);

    Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken);

    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken);

    Task WriteAllTextAsync(string path, string text, CancellationToken cancellationToken);
}