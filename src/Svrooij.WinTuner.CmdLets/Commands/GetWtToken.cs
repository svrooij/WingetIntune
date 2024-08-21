using System.IdentityModel.Tokens.Jwt;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions.Authentication;
using Svrooij.PowerShell.DependencyInjection;

namespace Svrooij.WinTuner.CmdLets.Commands;

/// <summary>
/// <para type="synopsis">Get a token for graph</para>
/// <para type="description">This command will get a token for the graph api. The token is cached, so you can call this as often as you want.</para>
/// <para type="link" uri="https://wintuner.app/docs/wintuner-powershell/Get-WtToken">Documentation</para>
/// </summary>
/// <example>
/// <para type="description">Get token, show details and copy to clipboard</para>
/// <code>Get-WtToken -DecodeToken | Set-Clipboard</code>
/// </example>
[Cmdlet(VerbsCommon.Get, "WtToken", HelpUri = "https://wintuner.app/docs/wintuner-powershell/Get-WtToken")]
[OutputType(typeof(string))]
public class GetWtToken : BaseIntuneCmdlet
{
    /// <summary>
    /// Decode the token
    /// </summary>
    [Parameter(Mandatory = false, HelpMessage = "Decode the token")]
    public SwitchParameter DecodeToken { get; set; }
    
    /// <summary>
    /// Output the token to the logs?
    /// </summary>
    [Parameter(Mandatory = false, HelpMessage = "Output the token to the logs?")]
    public SwitchParameter ShowToken { get; set; }
    
    [ServiceDependency]
    private ILogger<GetWtToken>? _logger;

    /// <summary>
    /// Execute the command
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="cancellationToken"></param>
    protected override async Task ProcessAuthenticatedAsync(IAuthenticationProvider provider, CancellationToken cancellationToken)
    {
        var token = await ConnectWtWinTuner.GetTokenAsync(cancellationToken);
        if (ShowToken)
        {
            _logger?.LogInformation("Got token {Token}", token);
        }

        if (DecodeToken)
        {
            // Decode the jwt token and output the claims to the logs
            var jwt = new JwtSecurityToken(token);
            _logger?.LogInformation("Token claims: {@Claims}", jwt.Claims);
        }
        
        WriteObject(token);
    }
}
