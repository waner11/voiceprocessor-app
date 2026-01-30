using FluentAssertions;
using VoiceProcessor.Engines.Security;

namespace VoiceProcessor.Engines.Tests.Security;

public class ApiKeyEngineTests
{
    private readonly ApiKeyEngine _engine = new();

    [Fact]
    public void GenerateApiKey_ReturnsKeyWithVpPrefix()
    {
        // Arrange & Act
        var result = _engine.GenerateApiKey();

        // Assert
        result.FullKey.Should().StartWith("vp_");
    }

    [Fact]
    public void GenerateApiKey_ReturnsKeyOfCorrectLength()
    {
        // Arrange & Act
        var result = _engine.GenerateApiKey();

        // Assert
        result.FullKey.Length.Should().Be(35);
        result.FullKey.Should().StartWith("vp_");
        result.FullKey.Substring(3).Length.Should().Be(32);
    }

    [Fact]
    public void HashApiKey_ThenVerifyApiKey_ReturnsTrue()
    {
        // Arrange
        var result = _engine.GenerateApiKey();

        // Act
        var isValid = _engine.VerifyApiKey(result.FullKey, result.Hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyApiKey_WrongKey_ReturnsFalse()
    {
        // Arrange
        var result = _engine.GenerateApiKey();
        var wrongKey = "vp_wrongkeywrongkeywrongkeywrongk";

        // Act
        var isValid = _engine.VerifyApiKey(wrongKey, result.Hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ExtractPrefix_ReturnsFirst11Characters()
    {
        // Arrange
        var result = _engine.GenerateApiKey();

        // Act
        var prefix = _engine.ExtractPrefix(result.FullKey);

        // Assert
        prefix.Should().Be(result.Prefix);
        prefix.Length.Should().Be(11);
        prefix.Should().StartWith("vp_");
    }
}
