using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using System.Net;
using WingetIntune.Graph;

namespace WingetIntune.Tests.GraphExtensions;

public class MobileAppsRequestBuilderExtensionsTests
{
    [Fact]
    public async Task DeviceAppManagement_MobileApps_PostAsync_MakesCorrectRequest()
    {
        var appId = Guid.NewGuid().ToString();
        var token = Guid.NewGuid().ToString();
        var handler = Substitute.For<HttpMessageHandlerWrapper>();

        var app = new Win32LobApp
        {
            DisplayName = "Test App",
            Publisher = "Test Publisher",
            IsFeatured = true,
        };

        var response = new HttpResponseMessage(HttpStatusCode.Created);
        response.Content = new StringContent(win32LobAppResult);
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        handler.AddFakeResponse(
            $"https://graph.microsoft.com/beta/deviceAppManagement/mobileApps",
            HttpMethod.Post,
            @"{""@odata.type"":""#microsoft.graph.win32LobApp"",""displayName"":""Test App"",""isFeatured"":true,""publisher"":""Test Publisher""}",
            response);

        var httpClient = new HttpClient(handler);
        var graphServiceClient = new GraphServiceClient(httpClient, new Internal.Msal.StaticAuthenticationProvider(token));

        var result = await graphServiceClient.DeviceAppManagement.MobileApps.PostAsync(app, CancellationToken.None);
        Assert.Equal("9607b530-b530-9607-30b5-079630b50796", result!.Id);
    }

    [Fact]
    public async Task DeviceAppManagement_MobileApps_PatchAsync_MakesCorrectRequest()
    {
        var token = Guid.NewGuid().ToString();

        var appId = "9607b530-b530-9607-30b5-079630b50796";
        var handler = Substitute.For<HttpMessageHandlerWrapper>();

        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new StringContent(win32LobAppResult);
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        handler.AddFakeResponse(
            $"https://graph.microsoft.com/beta/deviceAppManagement/mobileApps/{appId}",
            HttpMethod.Patch,
            @"{""@odata.type"":""#microsoft.graph.win32LobApp"",""committedContentVersion"":""1""}",
            response);

        var httpClient = new HttpClient(handler);
        var graphServiceClient = new GraphServiceClient(httpClient, new Internal.Msal.StaticAuthenticationProvider(token));

        Win32LobApp? result = await graphServiceClient.DeviceAppManagement.MobileApps[appId].PatchAsync(new Win32LobApp { CommittedContentVersion = "1" }, CancellationToken.None);
        Assert.Equal("9607b530-b530-9607-30b5-079630b50796", result!.Id);
    }

    private const string win32LobAppResult = @"{
  ""@odata.type"": ""#microsoft.graph.win32LobApp"",
  ""id"": ""9607b530-b530-9607-30b5-079630b50796"",
  ""displayName"": ""Display Name value"",
  ""description"": ""Description value"",
  ""publisher"": ""Publisher value"",
  ""largeIcon"": {
    ""@odata.type"": ""microsoft.graph.mimeContent"",
    ""type"": ""Type value"",
    ""value"": ""dmFsdWU=""
  },
  ""createdDateTime"": ""2017-01-01T00:02:43.5775965-08:00"",
  ""lastModifiedDateTime"": ""2017-01-01T00:00:35.1329464-08:00"",
  ""isFeatured"": true,
  ""privacyInformationUrl"": ""https://example.com/privacyInformationUrl/"",
  ""informationUrl"": ""https://example.com/informationUrl/"",
  ""owner"": ""Owner value"",
  ""developer"": ""Developer value"",
  ""notes"": ""Notes value"",
  ""publishingState"": ""processing"",
  ""committedContentVersion"": ""Committed Content Version value"",
  ""fileName"": ""File Name value"",
  ""size"": 4,
  ""installCommandLine"": ""Install Command Line value"",
  ""uninstallCommandLine"": ""Uninstall Command Line value"",
  ""applicableArchitectures"": ""x86"",
  ""minimumFreeDiskSpaceInMB"": 8,
  ""minimumMemoryInMB"": 1,
  ""minimumNumberOfProcessors"": 9,
  ""minimumCpuSpeedInMHz"": 4,
  ""rules"": [
    {
      ""@odata.type"": ""microsoft.graph.win32LobAppRegistryRule"",
      ""ruleType"": ""requirement"",
      ""check32BitOn64System"": true,
      ""keyPath"": ""Key Path value"",
      ""valueName"": ""Value Name value"",
      ""operationType"": ""exists"",
      ""operator"": ""equal"",
      ""comparisonValue"": ""Comparison Value value""
    }
  ],
  ""installExperience"": {
    ""@odata.type"": ""microsoft.graph.win32LobAppInstallExperience"",
    ""runAsAccount"": ""user"",
    ""deviceRestartBehavior"": ""allow""
  },
  ""returnCodes"": [
    {
      ""@odata.type"": ""microsoft.graph.win32LobAppReturnCode"",
      ""returnCode"": 10,
      ""type"": ""success""
    }
  ],
  ""msiInformation"": {
    ""@odata.type"": ""microsoft.graph.win32LobAppMsiInformation"",
    ""productCode"": ""Product Code value"",
    ""productVersion"": ""Product Version value"",
    ""upgradeCode"": ""Upgrade Code value"",
    ""requiresReboot"": true,
    ""packageType"": ""perUser"",
    ""productName"": ""Product Name value"",
    ""publisher"": ""Publisher value""
  },
  ""setupFilePath"": ""Setup File Path value"",
  ""minimumSupportedWindowsRelease"": ""Minimum Supported Windows Release value""
}";
}
