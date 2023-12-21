using Microsoft.Extensions.Configuration;

namespace WingetIntune.Cli.Configuration;

internal class ControlableLoggingSource : IConfigurationSource
{
    internal static readonly ControlableLoggingProvider Provider = new ControlableLoggingProvider();

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return Provider;
    }
}
