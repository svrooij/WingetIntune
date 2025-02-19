using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WingetIntune.Cli.Configuration;

internal class ControlableLoggingProvider : ConfigurationProvider
{
    private const string FormatterNameKey = "Logging:Console:FormatterName";

    internal ControlableLoggingProvider()
    {
        Data.Add("Logging:LogLevel:Default", "Information");
        Data.Add("Logging:LogLevel:Microsoft", "Warning");
        Data.Add("Logging:LogLevel:Microsoft.Extensions.Http.DefaultHttpClientFactory", "Warning");
        Data.Add("Logging:LogLevel:WingetIntune.Os.ProcessManager", "Warning");
        Data.Add("Logging:LogLevel:WingetIntune.Os.DefaultFileManager", "Warning");
        Data.Add("Logging:LogLevel:WingetIntune.WingetManager", "Information");
        Data.Add("Logging:LogLevel:System", "Warning");
        Data.Add("Logging:LogLevel:WingetIntune", "Information");
        Data.Add("Logging:LogLevel:Winget", "Information");

        Data.Add(FormatterNameKey, "simple");
        Data.Add("Logging:Console:FormatterOptions:SingleLine", "true");
    }

    public void SetLogLevel(LogLevel logLevel, string logKey = "Default")
    {
        var key = $"Logging:LogLevel:{logKey}";
        if (Data.ContainsKey(key))
        {
            base.Set(key, logLevel.ToString());
            OnReload();
        }
    }

    public void SetOutputFormat(string format)
    {
        if (format != "simple" && format != "json")
        {
            throw new ArgumentException("Invalid format", nameof(format));
        }
        base.Set(FormatterNameKey, format);
        OnReload();
    }

    public void SetVerbose()
    {
        var keys = Data.Keys.Where(k => k.StartsWith("Logging:LogLevel:") && !k.Contains("Microsoft.Extensions.Http.DefaultHttpClientFactory")).ToList();
        foreach (var key in keys)
        {
            base.Set(key, "Debug");
        }
        OnReload();
    }

    public override void Set(string key, string? value)
    {
        throw new InvalidOperationException("This configuration provider is read-only");
    }
}
