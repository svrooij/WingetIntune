using Microsoft.Extensions.Logging;
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using Microsoft.Graph.Beta.Models.ODataErrors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingetIntune.Models;

namespace WingetIntune.Graph;
public class GraphStoreAppUploader
{
    private readonly ILogger<GraphStoreAppUploader> logger;
    private readonly IFileManager fileManager;
    private readonly Internal.MsStore.MicrosoftStoreClient microsoftStoreClient;
    private readonly Mapper mapper = new();

    public GraphStoreAppUploader(ILogger<GraphStoreAppUploader> logger, IFileManager fileManager, Internal.MsStore.MicrosoftStoreClient microsoftStoreClient)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fileManager);
        ArgumentNullException.ThrowIfNull(microsoftStoreClient);
#endif
        this.logger = logger;
        this.fileManager = fileManager;
        this.microsoftStoreClient = microsoftStoreClient;
    }

    public Task<string?> GetStoreIdForNameAsync(string searchstring, CancellationToken cancellationToken)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(searchstring);
#endif
        return microsoftStoreClient.GetPackageIdForFirstMatchAsync(searchstring, cancellationToken);
    }

    public async Task<WinGetApp?> CreateStoreAppAsync(GraphServiceClient graphServiceClient, string packageId, CancellationToken cancellationToken)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(graphServiceClient);
        ArgumentException.ThrowIfNullOrEmpty(packageId);
        ArgumentNullException.ThrowIfNull(cancellationToken);
#endif
        var catalog = await microsoftStoreClient.GetDisplayCatalogAsync(packageId!, cancellationToken);
        ArgumentNullException.ThrowIfNull(catalog);
        if (!(catalog.Products?.Count() > 0))
        {
            logger.LogError("No products found for {packageId}", packageId);
            return null;
        }

        var app = mapper.ToWinGetApp(catalog!);

        try
        {
            var imagePath = Path.GetTempFileName();
            var uriPart = catalog.Products.First()?.LocalizedProperties.FirstOrDefault()?.Images?.FirstOrDefault(i => i.Height == 300 && i.Width == 300)?.Uri; // && i.ImagePurpose.Equals("Tile", StringComparison.OrdinalIgnoreCase)
            if (uriPart is null)
            {
                logger.LogWarning("No image found for {packageId}", packageId);
            }
            else
            {
                var imageUrl = $"http:{uriPart}";
                await fileManager.DownloadFileAsync(imageUrl, imagePath, overrideFile: true, cancellationToken: cancellationToken);
                app.LargeIcon = new MimeContent
                {
                    Type = "image/png",
                    Value = await fileManager.ReadAllBytesAsync(imagePath, cancellationToken)
                };
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error downloading image for {packageId}", packageId);
        }

        logger.LogInformation("Creating new WinGetApp (MsStore) for {packageId}", packageId);

        try
        {
            var createdApp = await graphServiceClient.DeviceAppManagement.MobileApps.PostAsync(app, cancellationToken);
            logger.LogInformation("MsStore app {packageIdentifier} created in Intune {appId}", createdApp?.PackageIdentifier, createdApp?.Id);
            return createdApp;
        }
        catch (ODataError ex)
        {
            logger.LogError(ex, "Error publishing app {message}", ex.Error?.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing app");
            throw;
        }
    }
}
