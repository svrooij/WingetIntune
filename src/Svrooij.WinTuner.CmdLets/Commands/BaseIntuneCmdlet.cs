using Microsoft.Extensions.Options;
using Microsoft.Graph.Beta;
using Microsoft.Kiota.Abstractions.Authentication;
using Svrooij.PowerShell.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Svrooij.WinTuner.CmdLets.Commands;
/// <summary>
/// Base class for all cmdlets that need to connect to Graph.
/// </summary>
public abstract class BaseIntuneCmdlet : DependencyCmdlet<Startup>
{

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 20,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Use a managed identity to connect to Intune")]
    public bool UseManagedIdentity { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 21,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Use default Azure Credentials from Azure.Identity to connect to Intune")]
    public bool UseDefaultAzureCredential { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
        Mandatory = false,
        Position = 22,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Use a token from another source to connect to Intune")]
    public string? Token { get; set; }

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
    internal static string[] DefaultScopes { get; } = new[] { "DeviceManagementConfiguration.ReadWrite.All", "DeviceManagementApps.ReadWrite.All" };

    internal void ValidateAuthenticationParameters()
    {
        if (!string.IsNullOrEmpty(Token))
        {
            return;
        }

        if (UseManagedIdentity || UseDefaultAzureCredential)
        {
            return;
        }

        if (!string.IsNullOrEmpty(Username))
        {
            return;
        }

        throw new ArgumentException($"Use `{nameof(Token)}`, `{nameof(UseManagedIdentity)}`, `{nameof(UseDefaultAzureCredential)}` or `{nameof(Username)}` to select the graph connection type", nameof(ParameterSetName));
    }

    internal IAuthenticationProvider CreateAuthenticationProvider(string[]? scopes = null)
    {
        if (!string.IsNullOrEmpty(Token))
        {
            return new WingetIntune.Internal.Msal.StaticAuthenticationProvider(Token);
        }

        if (UseManagedIdentity || UseDefaultAzureCredential)
        {
            // Maybe make which credentials to use configurable
            Azure.Core.TokenCredential credentials = UseManagedIdentity
                ? new Azure.Identity.ManagedIdentityCredential(ClientId)
                : new Azure.Identity.DefaultAzureCredential();
            return new Microsoft.Graph.Authentication.AzureIdentityAuthenticationProvider(credentials, null, null, scopes ?? DefaultScopes);
        }

        if (!string.IsNullOrEmpty(ClientId) && !string.IsNullOrEmpty(ClientSecret) && !string.IsNullOrEmpty(TenantId))
        {
            return new Microsoft.Graph.Authentication.AzureIdentityAuthenticationProvider(new Azure.Identity.ClientSecretCredential(TenantId, ClientId, ClientSecret, new Azure.Identity.ClientSecretCredentialOptions
            {
                TokenCachePersistenceOptions = new Azure.Identity.TokenCachePersistenceOptions
                {
                    Name = "WinTuner-PowerShell",
                    UnsafeAllowUnencryptedStorage = true,
                }
            }), scopes: scopes ?? DefaultScopes);
        }

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

        throw new NotImplementedException();
    }

    internal GraphServiceClient CreateGraphServiceClient(HttpClient httpClient, string[]? scopes = null)
    {
        var authenticationProvider = CreateAuthenticationProvider(scopes ?? DefaultScopes);
        var graphServiceClient = new GraphServiceClient(httpClient: httpClient, authenticationProvider: authenticationProvider, baseUrl: "https://graph.microsoft.com/beta");

        return graphServiceClient;
    }
}
