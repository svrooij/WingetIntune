using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using WingetIntune.Interfaces;
using WingetIntune.Intune;
[assembly: InternalsVisibleTo("WingetIntune.Tests")]
namespace WingetIntune;

public static class WingetServiceCollectionExtension
{
    public static IServiceCollection AddWingetServices(this IServiceCollection services)
    {
        services.ConfigureHttpClientDefaults(config =>
        {
            config.ConfigureHttpClient(client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "WinTuner");
                client.Timeout = TimeSpan.FromSeconds(180);
                // Set buffer size to 500MB
                // TODO: fix this in some other way, by not loading the whole file into memory.
                client.MaxResponseContentBufferSize = 1024L * 1024 * 50;
            });
        });

        services.AddKiotaHandlers();
        services.AddHttpClient<Graph.GraphClientFactory>((sp, client) =>
        {
            client.BaseAddress = new Uri("https://graph.microsoft.com/beta");
        }).AttachKiotaHandlers();

        services.AddTransient<IFileManager, Os.DefaultFileManager>();
        services.AddTransient<IProcessManager, Os.ProcessManager>();
        services.AddTransient<IWingetRepository, Implementations.WingetManager>();
        if (Environment.GetEnvironmentVariable("WINGETINTUNE_USE_PROCESS_PACKAGER") == "true")
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new NotSupportedException("Process packager is only supported on Windows");
            }
            services.AddTransient<IIntunePackager, ProcessIntunePackager>();
        }
        else
        {
            services.AddTransient<IIntunePackager, LibraryIntunePackager>();
        }

        // Old IAzureFileUploader implementation is not used anymore, it does not work correctly.
        //services.AddTransient<IAzureFileUploader, Implementations.AzCopyAzureUploader>();
        services.AddTransient<IAzureFileUploader, Implementations.ChunkedAzureFileUploader>();
        services.AddTransient<Intune.MetadataManager>();
        services.AddTransient<Graph.GraphAppUploader>();
        services.AddTransient<Graph.GraphStoreAppUploader>();
        services.AddSingleton<Internal.Msal.PublicClientAuth>();
        services.AddTransient<Internal.MsStore.MicrosoftStoreClient>();
        services.AddSingleton<IntuneManager>();

        return services;
    }

    private static IServiceCollection AddKiotaHandlers(this IServiceCollection services)
    {
        services.AddTransient<RetryHandler>();
        services.AddTransient<RedirectHandler>();
        services.AddTransient<ParametersNameDecodingHandler>();
        services.AddTransient<UserAgentHandler>();
        services.AddTransient<HeadersInspectionHandler>();
        return services;
    }

    private static IHttpClientBuilder AttachKiotaHandlers(this IHttpClientBuilder builder)
    {
        //builder.AddHttpMessageHandler<UriReplacementHandler<UriReplacementHandlerOption>>();
        //builder.AddHttpMessageHandler<RetryHandler>();
        builder.AddHttpMessageHandler<RedirectHandler>();
        builder.AddHttpMessageHandler<ParametersNameDecodingHandler>();
        builder.AddHttpMessageHandler<UserAgentHandler>();
        builder.AddHttpMessageHandler<HeadersInspectionHandler>();
        return builder;
    }
}
