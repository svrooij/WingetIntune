using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Graph.Beta;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Kiota.Abstractions.Authentication;
using WingetIntune.Graph;
using WingetIntune.Internal.MsStore;

namespace WingetIntune.Tests.Graph;

public class GraphStoreAppUploaderTests
{
    private readonly ILogger<GraphStoreAppUploader> _logger;
    private readonly IFileManager _fileManager;
    private readonly MicrosoftStoreClient _microsoftStoreClient;
    private readonly GraphServiceClient _graphServiceClient;
    private readonly GraphStoreAppUploader _graphStoreAppUploader;

    public GraphStoreAppUploaderTests()
    {
        _logger = Substitute.For<ILogger<GraphStoreAppUploader>>();
        _fileManager = Substitute.For<IFileManager>();
        _microsoftStoreClient = new MicrosoftStoreClient(new HttpClient(), new NullLogger<MicrosoftStoreClient>());
        _graphServiceClient = Substitute.For<GraphServiceClient>(Substitute.For<IAuthenticationProvider>(), null);
        _graphStoreAppUploader = new GraphStoreAppUploader(_logger, _fileManager, _microsoftStoreClient);
    }

    [Fact(Skip = "Not working, never did")]
    public async Task CreateStoreAppAsync_WithValidPackageId_CreatesAppSuccessfully()
    {
        // Arrange
        var packageId = "9NZVDKPMR9RD";
        var cancellationToken = CancellationToken.None;
        var expectedApp = new Microsoft.Graph.Beta.Models.WinGetApp
        {
            DisplayName = "Test App",
            Publisher = "Test Publisher",
            Description = "Test Description",
            PackageIdentifier = packageId,
            Id = Guid.NewGuid().ToString()
        };



        _graphServiceClient.DeviceAppManagement.MobileApps.PostAsync(Arg.Any<Microsoft.Graph.Beta.Models.WinGetApp>(), cancellationToken).Returns(Task.FromResult(expectedApp));

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
