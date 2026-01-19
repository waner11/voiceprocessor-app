using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Clients.Api.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IAuthManager _authManager;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IAuthManager authManager)
        : base(options, logger, encoder)
    {
        _authManager = authManager;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(AuthenticationSchemes.ApiKeyHeaderName, out var apiKeyHeader))
        {
            return AuthenticateResult.NoResult();
        }

        var apiKey = apiKeyHeader.ToString();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var validationResult = await _authManager.ValidateApiKeyAsync(apiKey);

        if (!validationResult.IsValid)
        {
            return AuthenticateResult.Fail(validationResult.Error ?? "Invalid API key");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, validationResult.UserId.ToString()!),
            new(ClaimTypes.Email, validationResult.Email ?? string.Empty),
            new("tier", validationResult.Tier ?? string.Empty),
            new("auth_method", "api_key")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
