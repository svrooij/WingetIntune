using Microsoft.Extensions.Logging;
using Microsoft.Graph.Beta;
using NSubstitute;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WingetIntune.Graph;
using WingetIntune.Models;
using Xunit;

namespace WingetIntune.Tests.Graph;

public class GraphStoreAppUploaderTests
{
    private readonly ILogger<GraphStoreAppUploader> _logger;
    private readonly IFileManager _fileManager;
    private readonly Internal.MsStore.MicrosoftStoreClient _microsoftStoreClient;
    private readonly GraphServiceClient _graphServiceClient;
    private readonly GraphStoreAppUploader _graphStoreAppUploader;

    public GraphStoreAppUploaderTests()
    {
        _logger = Substitute.For<ILogger<GraphStoreAppUploader>>();
        _fileManager = Substitute.For<IFileManager>();
        _microsoftStoreClient = Substitute.For<Internal.MsStore.MicrosoftStoreClient>(Substitute.For<HttpClient>(), _logger);
        _graphServiceClient = Substitute.For<GraphServiceClient>(Substitute.For<HttpClient>(), Substitute.For<IAuthenticationProvider>());
        _graphStoreAppUploader = new GraphStoreAppUploader(_logger, _fileManager, _microsoftStoreClient);
    }

    [Fact]
    public async Task CreateStoreAppAsync_WithValidPackageId_CreatesAppSuccessfully()
    {
        // Arrange
        var packageId = "9NZVDKPMR9RD";
        var cancellationToken = CancellationToken.None;
        var expectedApp = new WinGetApp
        {
            DisplayName = "Test App",
            Publisher = "Test Publisher",
            Description = "Test Description",
            PackageIdentifier = packageId,
            Id = Guid.NewGuid().ToString()
        };

        _microsoftStoreClient.GetDisplayCatalogAsync(packageId, cancellationToken).Returns(Task.FromResult(new DisplayCatalogResponse
        {
            Products = new[]
            {
                new Product
                {
                    ProductId = packageId,
                    LocalizedProperties = new[]
                    {
                        new LocalizedProperty
                        {
                            PublisherName = "Test Publisher",
                            DeveloperName = "Test Developer",
                            ProductTitle = "Test App",
                            ShortDescription = "Test Description",
                            Images = new[]
                            {
                                new Image
                                {
                                    Uri = "/test/image.png",
                                    Height = 300,
                                    Width = 300
                                }
                            }
                        }
                    }
                }
            }
        }));

        _graphServiceClient.DeviceAppManagement.MobileApps.PostAsync(Arg.Any<WinGetApp>(), cancellationToken).Returns(Task.FromResult(expectedApp));

        // Act
        var result = await _graphStoreAppUploader.CreateStoreAppAsync(_graphServiceClient, packageId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedApp.DisplayName, result.DisplayName);
        Assert.Equal(expectedApp.Publisher, result.Publisher);
        Assert.Equal(expectedApp.Description, result.Description);
        Assert.Equal(expectedApp.PackageIdentifier, result.PackageIdentifier);
        Assert.Equal(expectedApp.Id, result.Id);
    }
}
