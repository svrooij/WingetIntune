using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("WingetIntune.Tests")]

namespace WingetIntune.Internal.Msal;
internal class StaticAuthenticationProvider : IAuthenticationProvider
{
    private readonly string _token;

    public StaticAuthenticationProvider(string token)
    {
        _token = token;
    }

    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        var headers = new RequestHeaders
        {
            { "Authorization", $"Bearer {_token}" }
        };
        request.AddHeaders(headers);
        return Task.CompletedTask;
    }
}