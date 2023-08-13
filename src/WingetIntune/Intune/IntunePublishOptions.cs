using Azure.Core;
using Microsoft.Graph.Beta;

namespace WingetIntune.Intune;

public class IntunePublishOptions
{
    public TokenCredential? Credential { get; set; }
    public string? Token { get; set; }

    internal GraphServiceClient CreateGraphServiceClient(params string[] scopes)
    {

        return Credential is not null
            ? new GraphServiceClient(Credential, scopes)
            : new GraphServiceClient(new StaticAuthenticationProvider(Token!));
    }

    internal async Task<string> GetToken(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(Token))
        {
            return Token;
        }
        var result = await Credential!.GetTokenAsync(new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }), cancellationToken);
        return result.Token;
    }
}