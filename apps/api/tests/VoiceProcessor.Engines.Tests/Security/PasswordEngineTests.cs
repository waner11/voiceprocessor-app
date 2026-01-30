using FluentAssertions;
using VoiceProcessor.Engines.Security;

namespace VoiceProcessor.Engines.Tests.Security;

public class PasswordEngineTests
{
    private readonly PasswordEngine _engine = new();

    [Fact]
    public void HashPassword_ThenVerifyPassword_ReturnsTrue()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hash = _engine.HashPassword(password);
        var isValid = _engine.VerifyPassword(password, hash);

        // Assert
        isValid.Should().BeTrue();
        hash.Should().Contain(".");
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        // Arrange
        var correctPassword = "MySecurePassword123!";
        var wrongPassword = "WrongPassword456!";

        // Act
        var hash = _engine.HashPassword(correctPassword);
        var isValid = _engine.VerifyPassword(wrongPassword, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ProducesDifferentHashes()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hash1 = _engine.HashPassword(password);
        var hash2 = _engine.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
        
        _engine.VerifyPassword(password, hash1).Should().BeTrue();
        _engine.VerifyPassword(password, hash2).Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_MalformedHash_ReturnsFalse()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var malformedHash1 = "no-separator-here";
        var malformedHash3 = "";

        // Act & Assert
        _engine.VerifyPassword(password, malformedHash1).Should().BeFalse();
        _engine.VerifyPassword(password, malformedHash3).Should().BeFalse();
        
        var act = () => _engine.VerifyPassword(password, "invalid.base64!@#$%");
        act.Should().Throw<FormatException>();
    }
}
