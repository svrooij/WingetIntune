using Azure.Core;

namespace WingetIntune.Intune;

public class IntunePublishOptions
{
    public TokenCredential? Credential { get; set; }
    public string? Token { get; set; }

    public string? Tenant { get; set; }
    public string? Username { get; set; }
    public string[] Categories { get; set; } = Array.Empty<string>();
    public string[] AvailableFor { get; set; } = Array.Empty<string>();
    public string[] RequiredFor { get; set; } = Array.Empty<string>();
    public string[] UninstallFor { get; set; } = Array.Empty<string>();

    public bool AddAutoUpdateSetting { get; set; }
}
