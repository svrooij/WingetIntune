using Microsoft.Extensions.DependencyInjection;

namespace WingetIntune;
public static class WingetServiceCollectionExtension
{
    public static IServiceCollection AddWingetServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddTransient<IFileManager, Os.DefaultFileManager>();
        services.AddTransient<IProcessManager, Os.ProcessManager>();
        services.AddTransient<IWingetRepository, WingetManager>();
        services.AddSingleton<IntuneManager>();
        services.AddTransient<IAzureFileUploader, AzCopyAzureUploader>();
        services.AddSingleton<Internal.Msal.PublicClientAuth>();
        services.AddTransient<Internal.MsStore.MicrosoftStoreClient>();
        return services;
    }
}
