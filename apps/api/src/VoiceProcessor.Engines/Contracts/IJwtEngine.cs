namespace VoiceProcessor.Engines.Contracts;

public interface IJwtEngine
{
    JwtGenerationResult GenerateAccessToken(JwtGenerationContext context);
    JwtValidationResult ValidateAccessToken(string token);
    string GenerateRefreshToken();
}

public record JwtGenerationContext
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string Tier { get; init; }
    public string? Name { get; init; }
}

public record JwtGenerationResult
{
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
}

public record JwtValidationResult
{
    public required bool IsValid { get; init; }
    public Guid? UserId { get; init; }
    public string? Email { get; init; }
    public string? Tier { get; init; }
    public string? Error { get; init; }
}
