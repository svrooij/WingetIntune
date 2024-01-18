using Microsoft.Extensions.DependencyInjection;
using WingetIntune.Interfaces;
using WingetIntune.Intune;

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
            });
        });
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
        services.AddSingleton<Internal.Msal.PublicClientAuth>();
        services.AddTransient<Internal.MsStore.MicrosoftStoreClient>();
        services.AddSingleton<IntuneManager>();

        return services;
    }
}
