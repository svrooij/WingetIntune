using Microsoft.Extensions.DependencyInjection;
using WingetIntune.Interfaces;
using WingetIntune.Intune;

namespace WingetIntune;

public static class WingetServiceCollectionExtension
{
    public static IServiceCollection AddWingetServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddTransient<IFileManager, Os.DefaultFileManager>();
        services.AddTransient<IProcessManager, Os.ProcessManager>();
        services.AddTransient<IWingetRepository, Implementations.WingetManager>();
        services.AddTransient<IIntunePackager, ProcessIntunePackager>();
        services.AddSingleton<IntuneManager>();
        // Old IAzureFileUploader implementation is not used anymore, it does not work correctly.
        //services.AddTransient<IAzureFileUploader, Implementations.AzCopyAzureUploader>();
        services.AddTransient<IAzureFileUploader, Implementations.ChunkedAzureFileUploader>();
        services.AddSingleton<Internal.Msal.PublicClientAuth>();
        services.AddTransient<Internal.MsStore.MicrosoftStoreClient>();
        return services;
    }
}
