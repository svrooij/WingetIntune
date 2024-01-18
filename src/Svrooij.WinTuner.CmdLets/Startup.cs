using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Svrooij.PowerShell.DependencyInjection;
using Svrooij.PowerShell.DependencyInjection.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using WingetIntune;

namespace Svrooij.WinTuner.CmdLets;

public class Startup : PsStartup
{
    /// <inheritdoc/>
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<SvRooij.ContentPrep.Packager>();
        services.AddTransient<Winget.CommunityRepository.WingetRepository>();
        services.AddWingetServices();
    }

    /// <inheritdoc/>
    public override Action<PowerShellLoggerConfiguration> ConfigurePowerShellLogging()
    {
        return builder =>
        {
            builder.DefaultLevel = LogLevel.Information;
            builder.LogLevel.Add("System.Net.Http.HttpClient", LogLevel.Warning);
            builder.IncludeCategory = true;
            builder.StripNamespace = true;
        };
    }
}
