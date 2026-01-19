using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Security;

public class JwtEngine : IJwtEngine
{
    private readonly JwtOptions _options;
    private readonly TokenValidationParameters _validationParameters;

    public JwtEngine(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    public JwtGenerationResult GenerateAccessToken(JwtGenerationContext context)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, context.UserId.ToString()),
            new(ClaimTypes.Email, context.Email),
            new("tier", context.Tier),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrEmpty(context.Name))
            claims.Add(new Claim(ClaimTypes.Name, context.Name));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtGenerationResult
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }

    public JwtValidationResult ValidateAccessToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, _validationParameters, out _);

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var tier = principal.FindFirst("tier")?.Value;

            if (!Guid.TryParse(userId, out var userGuid))
                return new JwtValidationResult { IsValid = false, Error = "Invalid user ID claim" };

            return new JwtValidationResult
            {
                IsValid = true,
                UserId = userGuid,
                Email = email,
                Tier = tier
            };
        }
        catch (SecurityTokenExpiredException)
        {
            return new JwtValidationResult { IsValid = false, Error = "Token expired" };
        }
        catch (Exception ex)
        {
            return new JwtValidationResult { IsValid = false, Error = ex.Message };
        }
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
