using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace WingetIntune.Internal.MsStore;

/*
 * This code would not exists without the hard work of Sander Rozemuller, Thank you!
 * https://www.rozemuller.com/add-microsoft-store-app-with-icon-into-intune-automated/
 */

public partial class MicrosoftStoreClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MicrosoftStoreClient> _logger;

    public MicrosoftStoreClient(HttpClient httpClient, ILogger<MicrosoftStoreClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GetPackageIdForFirstMatchAsync(string searchString, CancellationToken cancellationToken)
    {
        LogSearchRequest(searchString);
        var result = await Search(searchString, cancellationToken);
        return result?.Data.FirstOrDefault()?.PackageIdentifier;
    }

    public async Task<DisplayCatalogResponse?> GetDisplayCatalogAsync(string packageId, CancellationToken cancellation)
    {
        //LogGetManifest(packageId);
        var url = $"https://displaycatalog.mp.microsoft.com/v7.0/products?bigIds={packageId}&market=US&languages=en-us";
        var response = await _httpClient.GetAsync(url, cancellation);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DisplayCatalogResponse>(cancellationToken: cancellation);
    }

    public async Task<MicrosoftStoreManifest?> GetManifestAsync(string packageId, CancellationToken cancellation)
    {
        LogGetManifest(packageId);
        var url = $"https://storeedgefd.dsx.mp.microsoft.com/v9.0/packageManifests/{packageId}";
        var response = await _httpClient.GetAsync(url, cancellation);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MicrosoftStoreManifest>(cancellationToken: cancellation);
    }

    public async Task<MicrosoftStoreDetails?> GetStoreDetailsAsync(string packageId, CancellationToken cancellation)
    {
        LogGetDetails(packageId);
        var url = $"https://apps.microsoft.com/store/api/ProductsDetails/GetProductDetailsById/{packageId}?hl=en-US&gl=US";
        //var url = $"https://storeedgefd.dsx.mp.microsoft.com/v9.0/productDetails/{packageId}?hl=en-US&gl=US";
        try
        {
            var response = await _httpClient.GetAsync(url, cancellation);
            response.EnsureSuccessStatusCode();
            var text = await response.Content.ReadAsStringAsync(cancellation);
            return await response.Content.ReadFromJsonAsync<MicrosoftStoreDetails>(cancellationToken: cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get details for {packageId} from Microsoft Store", packageId);
            return null;
        }
    }

    public async Task<MicrosoftStoreSearchResult?> Search(string searchString, CancellationToken cancellationToken)
    {
        var url = "https://storeedgefd.dsx.mp.microsoft.com/v9.0/manifestSearch";
        var body = new MicrosoftStoreSearchRequest
        {
            Query = new MicrosoftStoreSearchQuery
            {
                KeyWord = searchString
            }
        };

        var data = JsonSerializer.SerializeToUtf8Bytes(body);
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new ByteArrayContent(data)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MicrosoftStoreSearchResult>(cancellationToken: cancellationToken);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Searching for {searchString} in Microsoft Store")]
    private partial void LogSearchRequest(string searchString);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Getting manifest for {packageId} from Microsoft Store")]
    private partial void LogGetManifest(string packageId);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Getting details for {packageId} from Microsoft Store")]
    private partial void LogGetDetails(string packageId);
}
