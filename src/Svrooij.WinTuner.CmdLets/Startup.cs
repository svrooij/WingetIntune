using Microsoft.Extensions.DependencyInjection;
using Svrooij.PowerShell.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Svrooij.WinTuner.CmdLets;

public class Startup : PsStartup
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<SvRooij.ContentPrep.Packager>();
    }
}
