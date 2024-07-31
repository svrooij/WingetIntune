using Microsoft.Kiota.Abstractions.Authentication;
using Svrooij.PowerShell.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace Svrooij.WinTuner.CmdLets.Commands;
/// <summary>
/// Connecting WinTuner to your Intune instance
/// </summary>
[Cmdlet(VerbsCommunications.Connect, "WtWinTuner", DefaultParameterSetName = ParamSetInteractive)]
public class ConnectWtWinTuner : DependencyCmdlet<Startup>
{
    private const string DefaultClientId = "d5a8a406-3b1d-4069-91cc-d76acdd812fe";
    private const string DefaultClientCredentialScope = "https://graph.microsoft.com/.default";
    private const string ParamSetInteractive = "Interactive";
    private const string ParamSetClientCredentials = "ClientCredentials";
    
    internal static IAuthenticationProvider? AuthenticationProvider { get; private set; }

    /// <summary>
    /// Use a managed identity to connect to Intune
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 0,
        ParameterSetName = nameof(UseManagedIdentity),
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Use a managed identity to connect to Intune")]
    public SwitchParameter UseManagedIdentity { get; set; } = Environment.GetEnvironmentVariable("AZURE_USE_MANAGED_IDENTITY")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// Use default Azure Credentials from Azure.Identity to connect to Intune
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 0,
        ParameterSetName = nameof(UseDefaultCredentials),
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Use default Azure Credentials from Azure.Identity to connect to Intune")]
    public SwitchParameter UseDefaultCredentials { get; set; } = Environment.GetEnvironmentVariable("AZURE_USE_DEFAULT_CREDENTIALS")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>
    /// Use a token from another source to connect to Intune
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 0,
        ParameterSetName = nameof(Token),
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Use a token from another source to connect to Intune, this is the least preferred way to use")]
    public string? Token { get; set; } = Environment.GetEnvironmentVariable("AZURE_TOKEN");

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 2,
        ParameterSetName = ParamSetInteractive,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Disable Windows authentication broker")]
    public SwitchParameter NoBroker { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = true,
        Position = 0,
        ParameterSetName = ParamSetInteractive,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Use a username to trigger interactive login or SSO")]
    public string? Username { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 2,
        ParameterSetName = ParamSetInteractive,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Specify the tenant ID, optional. Loaded from `AZURE_TENANT_ID`")]
    [Parameter(
        Mandatory = true,
        Position = 2,
        ParameterSetName = ParamSetClientCredentials,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Specify the tenant ID. Loaded from `AZURE_TENANT_ID`")]
    public string? TenantId { get; set; } = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
               Mandatory = true,
               Position = 0,
               ParameterSetName = ParamSetClientCredentials,
               ValueFromPipeline = false,
               ValueFromPipelineByPropertyName = false,
               HelpMessage = "Specify the client ID, mandatory for Client Credentials flow. Loaded from `AZURE_CLIENT_ID`")]
    [Parameter(
        Mandatory = false,
        Position = 3,
        ParameterSetName = ParamSetInteractive,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Specify the alternative client ID, optional. Loaded from `AZURE_CLIENT_ID`")]
    public string? ClientId { get; set; } = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");


    /// <summary>
    /// 
    /// </summary>
    [Parameter(
               Mandatory = true,
               Position = 1,
               ParameterSetName = ParamSetClientCredentials,
               ValueFromPipeline = false,
               ValueFromPipelineByPropertyName = false,
               HelpMessage = "Specify the client secret. Loaded from `AZURE_CLIENT_SECRET`")]
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
    internal static string[] DefaultScopes { get; } = { "DeviceManagementConfiguration.ReadWrite.All", "DeviceManagementApps.ReadWrite.All" };
    
    [ServiceDependency]
    private ILogger<ConnectWtWinTuner>? logger;

    public override async Task ProcessRecordAsync(CancellationToken cancellationToken)
    {
        AuthenticationProvider = await CreateAuthenticationProvider(cancellationToken);

        // var ri = new RequestInformation(Method.GET, "https://graph.microsoft.com/test", new Dictionary<string, object>());
        // await AuthenticationProvider.AuthenticateRequestAsync(ri, cancellationToken: cancellationToken);
        // var headerValue = ri.Headers.TryGetValue("Authorization", out var values) ? values.FirstOrDefault() : "";
        // logger?.LogInformation("Got auth header value {HeaderValue}", headerValue);
    }

    internal async Task<IAuthenticationProvider> CreateAuthenticationProvider(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(Token))
        {
            return new WingetIntune.Internal.Msal.StaticAuthenticationProvider(Token);
        }

        var scope = (Scopes  ?? DefaultScopes)[0];

        if (UseManagedIdentity || UseDefaultCredentials)
        {
            // Maybe make which credentials to use configurable
            Azure.Core.TokenCredential credentials = UseManagedIdentity
                ? new Azure.Identity.ManagedIdentityCredential(ClientId)
                : new Azure.Identity.DefaultAzureCredential();
            return new Microsoft.Graph.Authentication.AzureIdentityAuthenticationProvider(credentials, null, null, DefaultClientCredentialScope);
        }

        if (ParameterSetName == ParamSetClientCredentials)
        {
            if (!string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(TenantId) &&
                !string.IsNullOrEmpty(ClientSecret))
            {
                return new Microsoft.Graph.Authentication.AzureIdentityAuthenticationProvider(
                    new Azure.Identity.ClientSecretCredential(TenantId, ClientId, ClientSecret,
                        new Azure.Identity.ClientSecretCredentialOptions
                        {
                            TokenCachePersistenceOptions = new Azure.Identity.TokenCachePersistenceOptions
                            {
                                Name = "WinTuner-PowerShell-CC",
                                UnsafeAllowUnencryptedStorage = true,
                            }
                        }), scopes: scope);
            }
            else
            {
                throw new ArgumentException("Not all parameters for client credentials are specified",
                    nameof(ClientId));
            }
        }

        if (ParameterSetName == ParamSetInteractive)
        {
            if (NoBroker || RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
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

                return new Microsoft.Graph.Authentication.AzureIdentityAuthenticationProvider(credential, scopes: Scopes ?? DefaultScopes);
            }
            
            return new WingetIntune.Internal.Msal.InteractiveAuthenticationProvider(new WingetIntune.Internal.Msal.InteractiveAuthenticationProviderOptions
            {
                ClientId = ClientId,
                TenantId = TenantId,
                Username = Username,
                Scopes = Scopes ?? DefaultScopes,
            });
        }

        // This should never happen, but just in case.
        // The ValidateAuthenticationParameters should have caught this.
        throw new NotImplementedException();
    }
}
