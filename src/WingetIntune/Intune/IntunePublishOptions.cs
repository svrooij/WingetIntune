using Azure.Core;

namespace WingetIntune.Intune;

public class IntunePublishOptions
{
    public TokenCredential? Credential { get; set; }
    public string? Token { get; set; }

    public string? Tenant { get; set; }
    public string? Username { get; set; }
}
