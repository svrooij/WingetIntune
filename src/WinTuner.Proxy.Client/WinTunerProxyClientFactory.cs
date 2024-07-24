using Microsoft.Extensions.Options;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace WinTuner.Proxy.Client;
internal class WinTunerProxyClientFactory
{
    private readonly WinTunerProxyClientOptions options;
    private readonly HttpClient httpClient;
    public WinTunerProxyClientFactory(HttpClient httpClient, IOptions<WinTunerProxyClientOptions> options)
    {
        this.httpClient = httpClient;
        this.options = options.Value;
        if (this.options.BaseAddress is not null)
        {
            this.httpClient.BaseAddress = this.options.BaseAddress;
        }
    }

    public WinTunerProxyClient GetClient(string? code = null) => new WinTunerProxyClient(new HttpClientRequestAdapter(new WinTunerProxyClientAuthenticationProvider(code ?? options.Code), httpClient: httpClient));
}
