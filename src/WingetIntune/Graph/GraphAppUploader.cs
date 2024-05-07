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
            await CreateNewContentVersionAsync(graphServiceClient, app!.Id!, intunePackageFile, cancellationToken);

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
                    await graphServiceClient.DeviceAppManagement.MobileApps[appId].DeleteAsync(cancellationToken: cancellationToken);
                }
                catch (Exception ex2)
                {
                    logger.LogError(ex2, "Error deleting app");
                }
            }
            throw;
        }
    }

    public async Task<Win32LobApp?> CreateNewContentVersionAsync(GraphServiceClient graphServiceClient, string appId, string intunePackageFile, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(graphServiceClient);
        ArgumentException.ThrowIfNullOrEmpty(appId);
        ArgumentException.ThrowIfNullOrEmpty(intunePackageFile);
#endif
        if (!fileManager.FileExists(intunePackageFile))
        {
            throw new FileNotFoundException("IntuneWin file not found", intunePackageFile);
        }
        logger.LogInformation("Creating new content version for app {appId}", appId);

        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        logger.LogDebug("Extracting intunewin file {file} to {tempFolder}", intunePackageFile, tempFolder);
        await fileManager.ExtractFileToFolderAsync(intunePackageFile, tempFolder, cancellationToken);

        // Load the metadata file
        var info = IntuneMetadata.GetApplicationInfo(await fileManager.ReadAllBytesAsync(IntuneMetadata.GetMetadataPath(tempFolder), cancellationToken))!;

        // Create the content version
        var contentVersion = await graphServiceClient.DeviceAppManagement.MobileApps[appId].GraphWin32LobApp.ContentVersions.PostAsync(new MobileAppContent(), cancellationToken: cancellationToken);
        //var contentVersion = await graphServiceClient.Intune_CreateWin32LobAppContentVersionAsync(appId, cancellationToken);
        logger.LogDebug("Created content version {id}", contentVersion!.Id);

        var mobileAppContentFileRequest = new MobileAppContentFile
        {
            Name = info.FileName,
            IsDependency = false,
            Size = info.UnencryptedContentSize,
            SizeEncrypted = fileManager.GetFileSize(IntuneMetadata.GetContentsPath(tempFolder)),
            Manifest = null,
        };

        logger.LogDebug("Creating content file {name} {size} {sizeEncrypted}", mobileAppContentFileRequest.Name, mobileAppContentFileRequest.Size, mobileAppContentFileRequest.SizeEncrypted);

        MobileAppContentFile? mobileAppContentFile = await graphServiceClient.DeviceAppManagement.MobileApps[appId].GraphWin32LobApp.ContentVersions[contentVersion.Id!].Files.PostAsync(mobileAppContentFileRequest, cancellationToken: cancellationToken);
        //var mobileAppContentFile = await graphServiceClient.Intune_CreateWin32LobAppContentVersionFileAsync(appId, contentVersion.Id!, mobileAppContentFileRequest, cancellationToken);

        logger.LogDebug("Created content file {id}", mobileAppContentFile?.Id);
        // Wait for a bit (it's generating the azure storage uri)
        await Task.Delay(3000, cancellationToken);

        MobileAppContentFile? updatedMobileAppContentFile = await graphServiceClient.DeviceAppManagement.MobileApps[appId].GraphWin32LobApp.ContentVersions[contentVersion.Id!].Files[mobileAppContentFile!.Id!].GetAsync(cancellationToken: cancellationToken);

        //MobileAppContentFile? updatedMobileAppContentFile = await graphServiceClient.Intune_GetWin32LobAppContentVersionFileAsync(appId,
        //    contentVersion!.Id!,
        //    mobileAppContentFile!.Id!,
        //    cancellationToken);

        logger.LogDebug("Loaded content file {id} {blobUri}", updatedMobileAppContentFile?.Id, updatedMobileAppContentFile?.AzureStorageUri);

        await azureFileUploader.UploadFileToAzureAsync(
            IntuneMetadata.GetContentsPath(tempFolder),
            new Uri(updatedMobileAppContentFile!.AzureStorageUri!),
            cancellationToken);

        logger.LogDebug("Uploaded content file {id} {blobUri}", updatedMobileAppContentFile.Id, updatedMobileAppContentFile.AzureStorageUri);
        fileManager.DeleteFileOrFolder(tempFolder);



        //await graphServiceClient.DeviceAppManagement.MobileApps[appId].GraphWin32LobApp.ContentVersions[contentVersion.Id!].Files[mobileAppContentFile!.Id!].Commit
        //    .PostAsync(new Microsoft.Graph.Beta.DeviceAppManagement.MobileApps.Item.GraphWin32LobApp.ContentVersions.Item.Files.Item.Commit.CommitPostRequestBody { 
        //        FileEncryptionInfo = mapper.ToGraphEncryptionInfo(info.EncryptionInfo)
        //    }, cancellationToken: cancellationToken);

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
