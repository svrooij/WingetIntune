using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetIntune.Cli.Configuration;
internal class ControlableLoggingSource : IConfigurationSource
{
    internal readonly static ControlableLoggingProvider Instance = new ControlableLoggingProvider();
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return Instance;
    }
}
