namespace WingetIntune;

public interface IAzureFileUploader
{
    Task UploadFileToAzureAsync(string filename, Uri sasUri, CancellationToken cancellationToken = default);
}
