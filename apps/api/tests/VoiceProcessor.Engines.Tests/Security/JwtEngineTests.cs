using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Engines.Security;

namespace VoiceProcessor.Engines.Tests.Security;

public class JwtEngineTests
{
    private readonly JwtEngine _engine;
    private readonly JwtOptions _options;

    public JwtEngineTests()
    {
        _options = new JwtOptions
        {
            SecretKey = "ThisIsATestSecretKeyThatIsLongEnoughForHS256Algorithm",
            Issuer = "VoiceProcessorTest",
            Audience = "VoiceProcessorTestAudience",
            AccessTokenExpirationMinutes = 15
        };
        _engine = new JwtEngine(Options.Create(_options));
    }

    #region GenerateAccessToken Tests

    [Fact]
    public void GenerateAccessToken_ValidContext_ProducesValidJwt()
    {
        // Arrange
        var context = new JwtGenerationContext
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Tier = "free",
            Name = "Test User"
        };

        // Act
        var result = _engine.GenerateAccessToken(context);

        // Assert
        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateAccessToken_ContainsCorrectClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var tier = "premium";
        var name = "John Doe";
        var context = new JwtGenerationContext
        {
            UserId = userId,
            Email = email,
            Tier = tier,
            Name = name
        };

        // Act
        var result = _engine.GenerateAccessToken(context);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);
        
        token.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
        token.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
        token.Claims.Should().Contain(c => c.Type == "tier" && c.Value == tier);
        token.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == name);
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateAccessToken_WithoutName_DoesNotIncludeNameClaim()
    {
        // Arrange
        var context = new JwtGenerationContext
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Tier = "free"
        };

        // Act
        var result = _engine.GenerateAccessToken(context);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);
        
        token.Claims.Should().NotContain(c => c.Type == ClaimTypes.Name);
    }

    #endregion

    #region ValidateAccessToken Tests

    [Fact]
    public void ValidateAccessToken_ValidToken_ReturnsCorrectClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var tier = "premium";
        var context = new JwtGenerationContext
        {
            UserId = userId,
            Email = email,
            Tier = tier
        };
        var generationResult = _engine.GenerateAccessToken(context);

        // Act
        var validationResult = _engine.ValidateAccessToken(generationResult.Token);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.UserId.Should().Be(userId);
        validationResult.Email.Should().Be(email);
        validationResult.Tier.Should().Be(tier);
        validationResult.Error.Should().BeNull();
    }

    [Fact]
    public void ValidateAccessToken_ExpiredToken_ReturnsInvalid()
    {
        // Arrange - Create engine with very short expiration
        var shortExpirationOptions = new JwtOptions
        {
            SecretKey = _options.SecretKey,
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            AccessTokenExpirationMinutes = -1 // Expired immediately
        };
        var shortExpirationEngine = new JwtEngine(Options.Create(shortExpirationOptions));
        
        var context = new JwtGenerationContext
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Tier = "free"
        };
        var generationResult = shortExpirationEngine.GenerateAccessToken(context);

        // Act
        var validationResult = _engine.ValidateAccessToken(generationResult.Token);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Error.Should().Be("Token expired");
    }

    [Fact]
    public void ValidateAccessToken_TamperedToken_ReturnsInvalid()
    {
        // Arrange
        var context = new JwtGenerationContext
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Tier = "free"
        };
        var generationResult = _engine.GenerateAccessToken(context);
        
        // Tamper with the token by changing a character
        var tamperedToken = generationResult.Token.Substring(0, generationResult.Token.Length - 5) + "XXXXX";

        // Act
        var validationResult = _engine.ValidateAccessToken(tamperedToken);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidateAccessToken_WrongIssuer_ReturnsInvalid()
    {
        // Arrange - Create engine with different issuer
        var wrongIssuerOptions = new JwtOptions
        {
            SecretKey = _options.SecretKey,
            Issuer = "WrongIssuer",
            Audience = _options.Audience,
            AccessTokenExpirationMinutes = 15
        };
        var wrongIssuerEngine = new JwtEngine(Options.Create(wrongIssuerOptions));
        
        var context = new JwtGenerationContext
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Tier = "free"
        };
        var generationResult = wrongIssuerEngine.GenerateAccessToken(context);

        // Act - Validate with original engine (different issuer)
        var validationResult = _engine.ValidateAccessToken(generationResult.Token);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidateAccessToken_WrongAudience_ReturnsInvalid()
    {
        // Arrange - Create engine with different audience
        var wrongAudienceOptions = new JwtOptions
        {
            SecretKey = _options.SecretKey,
            Issuer = _options.Issuer,
            Audience = "WrongAudience",
            AccessTokenExpirationMinutes = 15
        };
        var wrongAudienceEngine = new JwtEngine(Options.Create(wrongAudienceOptions));
        
        var context = new JwtGenerationContext
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Tier = "free"
        };
        var generationResult = wrongAudienceEngine.GenerateAccessToken(context);

        // Act - Validate with original engine (different audience)
        var validationResult = _engine.ValidateAccessToken(generationResult.Token);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Error.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GenerateRefreshToken Tests

    [Fact]
    public void GenerateRefreshToken_ReturnsNonNullNonEmptyString()
    {
        // Act
        var refreshToken = _engine.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_Returns64BytesBase64Encoded()
    {
        // Act
        var refreshToken = _engine.GenerateRefreshToken();

        // Assert
        var bytes = Convert.FromBase64String(refreshToken);
        bytes.Length.Should().Be(64);
    }

    [Fact]
    public void GenerateRefreshToken_EachCallProducesUniqueToken()
    {
        // Act
        var token1 = _engine.GenerateRefreshToken();
        var token2 = _engine.GenerateRefreshToken();
        var token3 = _engine.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
        token2.Should().NotBe(token3);
        token1.Should().NotBe(token3);
    }

    #endregion
}
