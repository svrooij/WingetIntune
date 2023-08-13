using Microsoft.Extensions.DependencyInjection;

namespace WingetIntune;
public static class WingetServiceCollectionExtension
{
    public static IServiceCollection AddWingetServices(this IServiceCollection services)
    {
        services.AddHttpClient<IFileManager, DefaultFileManager>();
        services.AddTransient<IProcessManager, ProcessManager>();
        services.AddTransient<IWingetRepository, WingetManager>();
        services.AddHttpClient<IntuneManager>();
        services.AddTransient<IAzureFileUploader, AzCopyAzureUploader>();
        services.AddSingleton<Internal.Msal.PublicClientAuth>();
        return services;
    }
}
