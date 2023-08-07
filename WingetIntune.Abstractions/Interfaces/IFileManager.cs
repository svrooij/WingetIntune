namespace WingetIntune;

public interface IFileManager
{
    void CreateFolder(string path);

    string CreateFolderForPackage(string parentFolder, string packageName, string packageVersion);

    bool FileExists(string path);

    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken);

    Task WriteAllTextAsync(string path, string text, CancellationToken cancellationToken);
}