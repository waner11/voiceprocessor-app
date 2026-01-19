using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace VoiceProcessor.Clients.Api.Authentication;

/// <summary>
/// Combined authentication handler that tries JWT first, then falls back to API key authentication.
/// This allows endpoints to accept either authentication method.
/// </summary>
public class JwtOrApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IAuthenticationHandlerProvider _handlers;

    public JwtOrApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IAuthenticationHandlerProvider handlers)
        : base(options, logger, encoder)
    {
        _handlers = handlers;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Try JWT Bearer authentication first
        var jwtHandler = await _handlers.GetHandlerAsync(Context, AuthenticationSchemes.JwtBearer);
        if (jwtHandler != null)
        {
            var jwtResult = await jwtHandler.AuthenticateAsync();
            if (jwtResult.Succeeded)
            {
                return jwtResult;
            }
        }

        // Fall back to API Key authentication
        var apiKeyHandler = await _handlers.GetHandlerAsync(Context, AuthenticationSchemes.ApiKey);
        if (apiKeyHandler != null)
        {
            var apiKeyResult = await apiKeyHandler.AuthenticateAsync();
            if (apiKeyResult.Succeeded)
            {
                return apiKeyResult;
            }
        }

        return AuthenticateResult.Fail("No valid authentication provided. Use Bearer token or X-API-Key header.");
    }
}
