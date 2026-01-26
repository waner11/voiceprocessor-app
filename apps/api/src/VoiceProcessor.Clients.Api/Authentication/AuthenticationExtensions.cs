using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VoiceProcessor.Engines.Security;

namespace VoiceProcessor.Clients.Api.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddVoiceProcessorAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is required");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = AuthenticationSchemes.JwtOrApiKey;
            options.DefaultChallengeScheme = AuthenticationSchemes.JwtOrApiKey;
        })
        .AddJwtBearer(AuthenticationSchemes.JwtBearer, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Only use cookie if no Authorization header present
                    if (!context.Request.Headers.ContainsKey("Authorization"))
                    {
                        if (context.Request.Cookies.TryGetValue("vp_access_token", out var token))
                        {
                            context.Token = token;
                        }
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Append("X-Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        })
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            AuthenticationSchemes.ApiKey, null)
        .AddScheme<AuthenticationSchemeOptions, JwtOrApiKeyAuthenticationHandler>(
            AuthenticationSchemes.JwtOrApiKey, null);

        services.AddAuthorization();

        return services;
    }
}
