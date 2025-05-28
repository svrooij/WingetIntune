using Microsoft.Extensions.Logging;
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using Microsoft.Graph.Beta.Models.ODataErrors;
using WingetIntune.Intune;
using WingetIntune.Models;

namespace WingetIntune.Graph;
public class GraphAppUploader
{
    private readonly ILogger<GraphAppUploader> logger;
    private readonly IFileManager fileManager;
    private readonly IAzureFileUploader azureFileUploader;
    private readonly Mapper mapper = new();

    public GraphAppUploader(ILogger<GraphAppUploader> logger, IFileManager fileManager, IAzureFileUploader azureFileUploader)
    {
        this.logger = logger;
        this.fileManager = fileManager;
        this.azureFileUploader = azureFileUploader;
    }

    public async Task<Win32LobApp?> CreateNewAppAsync(GraphServiceClient graphServiceClient, Win32LobApp win32LobApp, string intunePackageFile, string? logoPath = null, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(graphServiceClient);
        ArgumentNullException.ThrowIfNull(win32LobApp);
        ArgumentException.ThrowIfNullOrEmpty(intunePackageFile);
#endif
        if (!fileManager.FileExists(intunePackageFile))
        {
            throw new FileNotFoundException("IntuneWin file not found", intunePackageFile);
        }

        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            // Extract intunewin file to get the metadata file

            await fileManager.ExtractFileToFolderAsync(intunePackageFile, tempFolder, cancellationToken);
            var metadataFile = IntuneMetadata.GetMetadataPath(tempFolder);
            var intuneWinFile = IntuneMetadata.GetContentsPath(tempFolder);

            if (!fileManager.FileExists(metadataFile))
            {
                throw new FileNotFoundException("Metadata file not found", metadataFile);
            }
            if (!fileManager.FileExists(intuneWinFile))
            {
                throw new FileNotFoundException("IntuneWin file not found", intuneWinFile);
            }

            return await CreateNewAppAsync(graphServiceClient, win32LobApp, intuneWinFile, metadataFile, logoPath, cancellationToken);

        }
        finally
        {
            fileManager.DeleteFileOrFolder(tempFolder);
        }

    }

    public async Task<Win32LobApp?> CreateNewAppAsync(GraphServiceClient graphServiceClient, Win32LobApp win32LobApp, string partialIntuneWinFile, string metadataFile, string? logoPath = null, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(graphServiceClient);
        ArgumentNullException.ThrowIfNull(win32LobApp);
        ArgumentException.ThrowIfNullOrEmpty(partialIntuneWinFile);
#endif
        if (!fileManager.FileExists(partialIntuneWinFile))
        {
            throw new FileNotFoundException("IntuneWin file not found", partialIntuneWinFile);
        }
        if (win32LobApp.LargeIcon is null && !string.IsNullOrEmpty(logoPath) && fileManager.FileExists(logoPath))
        {
            win32LobApp.LargeIcon = new MimeContent
            {
                Type = "image/png",
                Value = await fileManager.ReadAllBytesAsync(logoPath, cancellationToken)
            };
        }
        logger.LogInformation("Creating new Win32LobApp");
        string? appId = null;
        try
        {
            Win32LobApp? app = await graphServiceClient.DeviceAppManagement.MobileApps.PostAsync(win32LobApp, cancellationToken);
            appId = app?.Id;

            // TODO: Maybe this delay is not needed? Have to test this.
            await Task.Delay(1000, cancellationToken);

            // Upload the content and update the app with the latest commited file id.
            await CreateNewContentVersionAsync(graphServiceClient, app!.Id!, partialIntuneWinFile, metadataFile, cancellationToken);

            // Load the app again to get the final state
            Win32LobApp? updatedApp = await graphServiceClient.DeviceAppManagement.MobileApps[app.Id].GetAsync(cancellationToken: cancellationToken) as Win32LobApp;

            return updatedApp;
        }
        catch (Microsoft.Identity.Client.MsalServiceException ex)
        {
            logger.LogError(ex, "Error publishing app, auth failed {message}", ex.Message);
            throw;
        }
        catch (ODataError ex)
        {
            logger.LogError(ex, "Error publishing app, deleting the remains {message}", ex.Error?.Message);
            if (appId != null)
            {
                try
                {
                    await graphServiceClient.DeviceAppManagement.MobileApps[appId].DeleteAsync(cancellationToken: cancellationToken);
                }
                catch (Exception ex2)
                {
                    logger.LogError(ex2, "Error deleting app");
                }
            }
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing app, deleting the remains");
            if (appId != null)
            {
                try
                {
                    // Do not use the cancellationToken here, we want to delete the app no matter what.
                    await graphServiceClient.DeviceAppManagement.MobileApps[appId].DeleteAsync(cancellationToken: CancellationToken.None);
                }
                catch (Exception ex2)
                {
                    logger.LogError(ex2, "Error deleting app");
                }
            }
            throw;
        }
    }

    public async Task<Win32LobApp?> CreateNewContentVersionAsync(GraphServiceClient graphServiceClient, string appId, string intuneWinFile, CancellationToken cancellationToken = default)
    {
        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            await fileManager.ExtractFileToFolderAsync(intuneWinFile, tempFolder, cancellationToken);
            var metadataFile = IntuneMetadata.GetMetadataPath(tempFolder);
            var partialIntuneWinFile = IntuneMetadata.GetContentsPath(tempFolder);

            if (!fileManager.FileExists(metadataFile))
            {
                throw new FileNotFoundException("Metadata file not found", metadataFile);
            }
            if (!fileManager.FileExists(intuneWinFile))
            {
                throw new FileNotFoundException("IntuneWin file not found", partialIntuneWinFile);
            }

            return await CreateNewContentVersionAsync(graphServiceClient, appId, partialIntuneWinFile, metadataFile, cancellationToken);
        }
        finally
        {
            fileManager.DeleteFileOrFolder(tempFolder);
        }
    }

    public async Task<Win32LobApp?> CreateNewContentVersionAsync(GraphServiceClient graphServiceClient, string appId, string partialIntuneWinFile, string metadataFile, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(graphServiceClient);
        ArgumentException.ThrowIfNullOrEmpty(appId);
        ArgumentException.ThrowIfNullOrEmpty(partialIntuneWinFile);
#endif
        if (!fileManager.FileExists(partialIntuneWinFile))
        {
            throw new FileNotFoundException("IntuneWin file not found", partialIntuneWinFile);
        }
        logger.LogInformation("Creating new content version for app {appId}", appId);

        // Load the metadata file
        var info = IntuneMetadata.GetApplicationInfo(await fileManager.ReadAllBytesAsync(metadataFile, cancellationToken))!;

        // Create the content version
        var contentVersion = await graphServiceClient.DeviceAppManagement.MobileApps[appId].GraphWin32LobApp.ContentVersions.PostAsync(new MobileAppContent(), cancellationToken: cancellationToken);
        //var contentVersion = await graphServiceClient.Intune_CreateWin32LobAppContentVersionAsync(appId, cancellationToken);
        logger.LogDebug("Created content version {id}", contentVersion!.Id);

        var mobileAppContentFileRequest = new MobileAppContentFile
        {
            Name = info.FileName,
            IsDependency = false,
            Size = info.UnencryptedContentSize,
            SizeEncrypted = fileManager.GetFileSize(partialIntuneWinFile),
            Manifest = null,
        };

        logger.LogDebug("Creating content file {name} {size} {sizeEncrypted}", mobileAppContentFileRequest.Name, mobileAppContentFileRequest.Size, mobileAppContentFileRequest.SizeEncrypted);

        MobileAppContentFile? mobileAppContentFile = await graphServiceClient.DeviceAppManagement.MobileApps[appId].GraphWin32LobApp.ContentVersions[contentVersion.Id!].Files.PostAsync(mobileAppContentFileRequest, cancellationToken: cancellationToken);
        logger.LogDebug("Created content file {id}", mobileAppContentFile?.Id);
        // Wait for a bit (it's generating the azure storage uri)
        await Task.Delay(3000, cancellationToken);

        MobileAppContentFile? updatedMobileAppContentFile = await graphServiceClient.DeviceAppManagement.MobileApps[appId].GraphWin32LobApp.ContentVersions[contentVersion.Id!].Files[mobileAppContentFile!.Id!].GetAsync(cancellationToken: cancellationToken);

        logger.LogDebug("Loaded content file {id} {blobUri}", updatedMobileAppContentFile?.Id, updatedMobileAppContentFile?.AzureStorageUri);

        await azureFileUploader.UploadFileToAzureAsync(
            partialIntuneWinFile,
            new Uri(updatedMobileAppContentFile!.AzureStorageUri!),
            cancellationToken);

        logger.LogDebug("Uploaded content file {id} {blobUri}", updatedMobileAppContentFile.Id, updatedMobileAppContentFile.AzureStorageUri);

        var encryptionInfo = mapper.ToFileEncryptionInfo(info.EncryptionInfo);
        await graphServiceClient.Intune_CommitWin32LobAppContentVersionFileAsync(appId,
            contentVersion!.Id!,
            mobileAppContentFile!.Id!,
            encryptionInfo,
            cancellationToken);

        MobileAppContentFile? commitedFile = await graphServiceClient.Intune_WaitForFinalCommitStateAsync(appId, contentVersion!.Id!, mobileAppContentFile!.Id!, cancellationToken);

        logger.LogInformation("Added content version {contentVersionId} to app {appId}", contentVersion.Id, appId);

        var app = await graphServiceClient.DeviceAppManagement.MobileApps[appId].PatchAsync(new Win32LobApp
        {
            CommittedContentVersion = contentVersion.Id,
        }, cancellationToken);

        return app;
    }
}
