using Microsoft.Extensions.Options;
using Microsoft.Graph.Beta;
using Microsoft.Kiota.Abstractions.Authentication;
using Svrooij.PowerShell.DependencyInjection;
using System;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Svrooij.WinTuner.CmdLets.Commands;
/// <summary>
/// Base class for all cmdlets that need to connect to Graph.
/// </summary>
public abstract class BaseIntuneCmdlet : DependencyCmdlet<Startup>
{
    private const string DefaultClientId = "d5a8a406-3b1d-4069-91cc-d76acdd812fe";
    private const string DefaultClientCredentialScope = "https://graph.microsoft.com/.default";

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 20,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Use a managed identity to connect to Intune")]
    public bool UseManagedIdentity { get; set; } = Environment.GetEnvironmentVariable("AZURE_USE_MANAGED_IDENTITY")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 21,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Use default Azure Credentials from Azure.Identity to connect to Intune")]
    public bool UseDefaultAzureCredential { get; set; } = Environment.GetEnvironmentVariable("AZURE_USE_DEFAULT_CREDENTIALS")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 22,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Use a token from another source to connect to Intune")]
    public string? Token { get; set; } = Environment.GetEnvironmentVariable("AZURE_TOKEN");

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 24,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Disable Windows authentication broker")]
    public bool NoBroker { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 25,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Use a username to trigger interactive login or SSO")]
    public string? Username { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 26,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Specify the tenant ID, optional for interactive, mandatory for Client Credentials flow. Loaded from `AZURE_TENANT_ID`")]
    public string? TenantId { get; set; } = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
               Mandatory = false,
               Position = 27,
               ValueFromPipeline = false,
               ValueFromPipelineByPropertyName = false,
               HelpMessage = "Specify the client ID, optional for interactive, mandatory for Client Credentials flow. Loaded from `AZURE_CLIENT_ID`")]
    public string? ClientId { get; set; } = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");


    /// <summary>
    /// 
    /// </summary>
    [Parameter(
               Mandatory = false,
               Position = 28,
               ValueFromPipeline = false,
               ValueFromPipelineByPropertyName = false,
               HelpMessage = "Specify the client secret, mandatory for Client Credentials flow. Loaded from `AZURE_CLIENT_SECRET`")]
    public string? ClientSecret { get; set; } = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 40,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Specify the scopes to request, default is `DeviceManagementConfiguration.ReadWrite.All`, `DeviceManagementApps.ReadWrite.All`")]
    public string[]? Scopes { get; set; } = Environment.GetEnvironmentVariable("AZURE_SCOPES")?.Split(' ');

    /// <summary>
    /// 
    /// </summary>
    internal static string[] DefaultScopes { get; } = new[] { "DeviceManagementConfiguration.ReadWrite.All", "DeviceManagementApps.ReadWrite.All" };

    internal void ValidateAuthenticationParameters()
    {
        if (!string.IsNullOrEmpty(Token))
        {
            return;
        }

        if (UseManagedIdentity || UseDefaultAzureCredential)
        {
            Scopes ??= new[] { DefaultClientCredentialScope };
            return;
        }

        if (!string.IsNullOrEmpty(Username))
        {
            return;
        }

        if (!string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret) && !string.IsNullOrEmpty(TenantId))
        {
            Scopes ??= new[] { DefaultClientCredentialScope };
            return;
        }

        throw new ArgumentException($"Use `{nameof(Token)}`, `{nameof(UseManagedIdentity)}`, `{nameof(UseDefaultAzureCredential)}` or `{nameof(Username)}` to select the graph connection type", nameof(ParameterSetName));
    }

    internal IAuthenticationProvider CreateAuthenticationProvider(string[]? scopes = null, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(Token))
        {
            return new WingetIntune.Internal.Msal.StaticAuthenticationProvider(Token);
        }

        var scope = (Scopes ?? scopes ?? DefaultScopes)[0];

        if (UseManagedIdentity || UseDefaultAzureCredential)
        {
            // Maybe make which credentials to use configurable
            Azure.Core.TokenCredential credentials = UseManagedIdentity
                ? new Azure.Identity.ManagedIdentityCredential(ClientId)
                : new Azure.Identity.DefaultAzureCredential();
            return new Microsoft.Graph.Authentication.AzureIdentityAuthenticationProvider(credentials, null, null, scope);
        }

        if (!string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(TenantId))
        {
            if (!string.IsNullOrEmpty(ClientSecret))
            {
                return new Microsoft.Graph.Authentication.AzureIdentityAuthenticationProvider(new Azure.Identity.ClientSecretCredential(TenantId, ClientId, ClientSecret, new Azure.Identity.ClientSecretCredentialOptions
                {
                    TokenCachePersistenceOptions = new Azure.Identity.TokenCachePersistenceOptions
                    {
                        Name = "WinTuner-PowerShell-CC",
                        UnsafeAllowUnencryptedStorage = true,
                    }
                }), scopes: scope);
            }

        }

        // Alternative interactive authentication in case the broker is not working as expected.
        if (!string.IsNullOrEmpty(Username) && (NoBroker || RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false))
        {
            var interactiveOptions = new Azure.Identity.InteractiveBrowserCredentialOptions
            {

                TenantId = TenantId,
                ClientId = ClientId ?? DefaultClientId,
                LoginHint = Username,
                RedirectUri = new Uri("http://localhost:12228/"),
                TokenCachePersistenceOptions = new Azure.Identity.TokenCachePersistenceOptions
                {
                    Name = "WinTuner-PowerShell",
                    UnsafeAllowUnencryptedStorage = true,
                },
                DisableAutomaticAuthentication = false,

            };
            interactiveOptions.AdditionallyAllowedTenants.Add("*");

            var credential = new Azure.Identity.InteractiveBrowserCredential(interactiveOptions);

            // This is to make sure it will get a token before we start using it.
            // This will trigger the login screen early in the process.
            //var result = credential.Authenticate(new Azure.Core.TokenRequestContext(scopes!, tenantId: TenantId), cancellationToken);

            return new Microsoft.Graph.Authentication.AzureIdentityAuthenticationProvider(credential, scopes: scopes ?? DefaultScopes);
        }

        // Interactive authentication with broker, seem to not always work as expected.
        if (!string.IsNullOrEmpty(Username))
        {
            return new WingetIntune.Internal.Msal.InteractiveAuthenticationProvider(new WingetIntune.Internal.Msal.InteractiveAuthenticationProviderOptions
            {
                ClientId = ClientId,
                TenantId = TenantId,
                Username = Username,
                Scopes = scopes ?? DefaultScopes,
            });
        }

        // This should never happen, but just in case.
        // The ValidateAuthenticationParameters should have caught this.
        throw new NotImplementedException();
    }
}
