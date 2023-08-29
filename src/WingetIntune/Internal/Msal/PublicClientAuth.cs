using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace WingetIntune.Internal.Msal;

public sealed class PublicClientAuth : IAuthenticationProvider
{
    private readonly PublicClientOptions _options;
    private readonly IPublicClientApplication publicClientApplication;
    private readonly ILogger<PublicClientAuth> logger;
    private bool CacheLoaded = false;
    private const string DefaultClientId = "d5a8a406-3b1d-4069-91cc-d76acdd812fe";
    private AccountSuggestion? accountSuggestion;
    private string[] Scopes = IntuneManager.RequiredScopes;

    public PublicClientAuth(ILogger<PublicClientAuth> logger, IOptions<PublicClientOptions>? options)
    {
        if (options is null || string.IsNullOrWhiteSpace(options.Value.ClientId))
        {
            _options = new PublicClientOptions { ClientId = DefaultClientId };
        }
        else
        {
            _options = options.Value;
        }
        if (_options.UseBroker)
        {
            publicClientApplication = PublicClientApplicationBuilder
                .Create(_options.ClientId)
                .WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows) { Title = "Winget Intune uploader" })
                .Build();
        }
        else
        {
            publicClientApplication = PublicClientApplicationBuilder
                .Create(_options.ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdMultipleOrgs)
                .WithDefaultRedirectUri()
                .Build();
        }

        this.logger = logger;
    }

    private async Task LoadCache()
    {
        if (CacheLoaded)
        {
            return;
        }
        var storageProperties = new StorageCreationPropertiesBuilder(".accounts", Path.Combine(Path.GetTempPath(), "wingetintune"))

            .Build();
        var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
        cacheHelper.RegisterCache(publicClientApplication.UserTokenCache);
    }

    public void SetAccountSuggestion(AccountSuggestion accountSuggestion)
    {
        this.accountSuggestion = accountSuggestion;
    }

    public void SetScopes(string[] scopes)
    {
        Scopes = scopes;
    }

    public async Task<AuthenticationResult> AccuireTokenAsync(IEnumerable<string> scopes, string? tenantId = null, string? userId = null, CancellationToken cancellationToken = default)
    {
        await LoadCache();
        var accounts = await publicClientApplication.GetAccountsAsync();
        bool tenantIsGuid = Guid.TryParse(tenantId, out _);
        var account = accounts.FirstOrDefault(a => (string.IsNullOrWhiteSpace(tenantId) || tenantIsGuid == false || a.HomeAccountId.TenantId == tenantId)
            && (string.IsNullOrEmpty(userId) || a.Username.Equals(userId, StringComparison.InvariantCultureIgnoreCase)));

        try
        {
            var authResult = await publicClientApplication.AcquireTokenSilent(scopes, account).ExecuteAsync(cancellationToken);
            logger.LogTrace("Acquired token silently {@scopes} {tenantId} {username}", scopes, tenantId ?? authResult.TenantId, authResult.Account.Username);
            return authResult;
        }
        catch (MsalUiRequiredException)
        {
            return await AcquireTokenInteractiveAsync(scopes, tenantId, userId, cancellationToken);
        }
    }

    public async Task<AuthenticationResult> AcquireTokenInteractiveAsync(IEnumerable<string> scopes, string? tenantId = null, string? userId = null, CancellationToken cancellationToken = default)
    {
        if (!CacheLoaded)
            await LoadCache();
        logger.LogInformation("Acquiring token interactively {@scopes} {tenantId} {userId}", scopes, tenantId, userId);
        var builder = publicClientApplication.AcquireTokenInteractive(scopes);
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            builder = builder.WithTenantId(tenantId);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            builder = builder.WithLoginHint(userId);
        }

        if (_options.UseBroker)
        {
            builder = builder.WithParentActivityOrWindow(BrokerHandle.GetConsoleOrTerminalWindow());
        }

        return await builder.ExecuteAsync(cancellationToken);
    }

    public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (request.URI.Host == "graph.microsoft.com")
        {
            var token = await AccuireTokenAsync(Scopes, accountSuggestion?.TenantId, accountSuggestion?.Username, cancellationToken);
            var headers = new RequestHeaders
            {
                { "Authorization", $"Bearer {token.AccessToken}" }
            };
            request.AddHeaders(headers);
        }
    }
}

public class PublicClientOptions
{
    public string? ClientId { get; set; }
    public bool UseBroker { get; set; } = true;
}

public record AccountSuggestion(string? TenantId, string? Username);
