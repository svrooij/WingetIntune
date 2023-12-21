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
        services.AddSingleton<IntuneManager>();
        // Old IAzureFileUploader implementation is not used anymore, it does not work correctly.
        //services.AddTransient<IAzureFileUploader, Implementations.AzCopyAzureUploader>();
        services.AddTransient<IAzureFileUploader, Implementations.ChunkedAzureFileUploader>();
        services.AddSingleton<Internal.Msal.PublicClientAuth>();
        services.AddTransient<Internal.MsStore.MicrosoftStoreClient>();
        return services;
    }
}
