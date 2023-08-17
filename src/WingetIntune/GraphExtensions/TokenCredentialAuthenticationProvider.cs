using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace WingetIntune.GraphExtensions;

internal class TokenCredentialAuthenticationProvider : IAuthenticationProvider
{
    private readonly Azure.Core.TokenCredential credential;
    private readonly ILogger<TokenCredentialAuthenticationProvider> logger;
    private readonly string[] AppScopes = new[] { "https://graph.microsoft.com/.default" };
    private readonly string[] UserScopes;

    public TokenCredentialAuthenticationProvider(Azure.Core.TokenCredential credential, string[] userScopes, ILogger<TokenCredentialAuthenticationProvider>? logger = null)
    {
        this.credential = credential;
        UserScopes = userScopes;
        this.logger = logger ?? new NullLogger<TokenCredentialAuthenticationProvider>();
    }

    public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        var token = await GetUserScopeTokenAsync(cancellationToken) ?? await GetAppScopeTokenAsync(cancellationToken);
        var headers = new RequestHeaders
        {
            { "Authorization", $"Bearer {token}" }
        };
        request.AddHeaders(headers);
    }

    public async Task<string?> GetUserScopeTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(UserScopes), cancellationToken);
            return result.Token;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get token using delegation scopes");
        }
        return null;
    }

    public async Task<string> GetAppScopeTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(AppScopes), cancellationToken);
            return result.Token;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get token using app scopes");
            throw;
        }
    }
}
