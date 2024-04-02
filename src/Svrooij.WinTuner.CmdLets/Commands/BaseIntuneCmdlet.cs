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
        Position = 21,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Use a token from another source to connect to Intune")]
    public string? Token { get; set; }

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
        HelpMessage = "Specify the tenant ID, if you want to use another tenant then your home tenant")]
    public string? TenantId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Parameter(
               Mandatory = false,
               Position = 27,
               ValueFromPipeline = false,
               ValueFromPipelineByPropertyName = false,
               HelpMessage = "(optionally) Use a different client ID, apart from the default configured one.")]
    public string? ClientId { get; set; }

    internal string[] DefaultScopes { get; set; } = new[] { "DeviceManagementConfiguration.ReadWrite.All", "DeviceManagementApps.ReadWrite.All" };

    internal void ValidateAuthenticationParameters()
    {
        if (!string.IsNullOrEmpty(Token))
        {
            return;
        }

        if (UseManagedIdentity)
        {
            return;
        }

        if (!string.IsNullOrEmpty(Username))
        {
            return;
        }

        throw new ArgumentException($"Use `{nameof(Token)}`, `{nameof(UseManagedIdentity)}` or `{nameof(Username)}` to select the graph connection type", nameof(ParameterSetName));
    }

    internal IAuthenticationProvider CreateAuthenticationProviderAsync(string[]? scopes = null)
    {
        if (!string.IsNullOrEmpty(Token))
        {
            return new WingetIntune.Internal.Msal.StaticAuthenticationProvider(Token);
        }

        if (UseManagedIdentity)
        {
            // Maybe make which credentials to use configurable
            var credentials = new Azure.Identity.DefaultAzureCredential();
            return new Microsoft.Graph.Authentication.AzureIdentityAuthenticationProvider(credentials, null, null, scopes ?? DefaultScopes);
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
        var authenticationProvider = CreateAuthenticationProviderAsync(scopes ?? DefaultScopes);
        var graphServiceClient = new GraphServiceClient(httpClient: httpClient, authenticationProvider: authenticationProvider, baseUrl: "https://graph.microsoft.com/beta");

        return graphServiceClient;
    }
}
