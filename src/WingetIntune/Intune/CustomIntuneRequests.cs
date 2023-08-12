using Microsoft.Graph.Beta.Models;
using Microsoft.Graph.Beta.Models.ODataErrors;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using System.Text;
using System.Text.Json;

namespace WingetIntune.Intune;

internal static class CustomIntuneRequests
{
    public static RequestInformation CreateContentVersionRequest(string appId)
    {
        var requestInfo = new RequestInformation
        {
            HttpMethod = Method.POST,
            URI = new Uri($"https://graph.microsoft.com/beta/deviceAppManagement/mobileApps/{appId}/microsoft.graph.win32LobApp/contentVersions"),
            //UrlTemplate = "{baseUrl}/deviceAppManagement/mobileApps/{appId}/microsoft.graph.win32LobApp/contentVersions",
            //PathParameters = new Dictionary<string, object>
            //{
            //    {"appId", appId},
            //},
        };
        requestInfo.Headers.Add("Content-Type", "application/json");
        requestInfo.Content = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        return requestInfo;
    }

    public static RequestInformation CreateMobileAppContentFileRequest(this IRequestAdapter requestAdapter, string appId, string contentVersion, MobileAppContentFile mobileAppContentFile)
    {
        var requestInfo = new RequestInformation
        {
            HttpMethod = Method.POST,

            //UrlTemplate = "{baseUrl}/deviceAppManagement/mobileApps/{appId}/microsoft.graph.win32LobApp/contentVersions/{contentVersion}/files",
            //PathParameters = new Dictionary<string, object>
            //{
            //    {"appId", appId},
            //    {"contentVersion", contentVersion},
            //},
            URI = new Uri($"https://graph.microsoft.com/beta/deviceAppManagement/mobileApps/{appId}/microsoft.graph.win32LobApp/contentVersions/{contentVersion}/files"),
        };
        requestInfo.SetContentFromParsable(requestAdapter, "application/json", mobileAppContentFile);
        return requestInfo;
    }

    public static RequestInformation GetMobileAppContentFileRequest(string appId, string contentVersion, string mobileAppContentFileId)
    {
        var requestInfo = new RequestInformation
        {
            HttpMethod = Method.GET,
            //UrlTemplate = "{baseUrl}/deviceAppManagement/mobileApps/{appId}/microsoft.graph.win32LobApp/contentVersions/{contentVersion}/files/{mobileAppContentFileId}",
            //PathParameters = new Dictionary<string, object>
            //{
            //    {"appId", appId},
            //    {"contentVersion", contentVersion},
            //    {"mobileAppContentFileId", mobileAppContentFileId},
            //},
            URI = new Uri($"https://graph.microsoft.com/beta/deviceAppManagement/mobileApps/{appId}/microsoft.graph.win32LobApp/contentVersions/{contentVersion}/files/{mobileAppContentFileId}"),
        };
        return requestInfo;
    }

    public static RequestInformation CommitFileRequest(string appId, string contentVersion, string mobileAppContentFileId, FileEncryptionInfo encryptionInfo)
    {
        var body = new MobileAppContentFileCommitBody
        {
            FileEncryptionInfo = encryptionInfo,
        };

        var data = JsonSerializer.SerializeToUtf8Bytes(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var requestInfo = new RequestInformation
        {
            HttpMethod = Method.POST,
            Content = new MemoryStream(data),
            URI = new Uri($"https://graph.microsoft.com/beta/deviceAppManagement/mobileApps/{appId}/microsoft.graph.win32LobApp/contentVersions/{contentVersion}/files/{mobileAppContentFileId}/commit"),
        };
        requestInfo.Headers.Add("Content-Type", "application/json");
        return requestInfo;
    }

    public static Dictionary<string, ParsableFactory<IParsable>> ErrorMapping => new Dictionary<string, ParsableFactory<IParsable>> {
                {"4XX", ODataError.CreateFromDiscriminatorValue},
                {"5XX", ODataError.CreateFromDiscriminatorValue},
            };
}

public class MobileAppContentFileCommitBody
{
    public FileEncryptionInfo FileEncryptionInfo { get; set; }
}

public class FileEncryptionInfo
{
    public string EncryptionKey { get; set; }
    public string InitializationVector { get; set; }
    public string Mac { get; set; }
    public string MacKey { get; set; }
    public string ProfileIdentifier { get; set; }
    public string FileDigest { get; set; }
    public string FileDigestAlgorithm { get; set; }
}