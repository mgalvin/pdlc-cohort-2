using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Todo.Api.Infrastructure;

/// <summary>
/// Optional test authentication handler. When TestAuth:Enabled is true (e.g. in integration tests),
/// authenticates all requests so that authorization-protected endpoints can be exercised.
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            if (Configuration != null && Configuration.GetValue<bool>("TestAuth:Enabled"))
            {
                var identity = new ClaimsIdentity(SchemeName);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, SchemeName);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
        catch
        {
            // Fall through to NoResult when config is invalid
        }

        return Task.FromResult(AuthenticateResult.NoResult());
    }
}
