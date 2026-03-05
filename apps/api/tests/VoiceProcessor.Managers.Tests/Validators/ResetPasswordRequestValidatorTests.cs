using FluentAssertions;
using FluentValidation;
using VoiceProcessor.Clients.Api.Validators;
using VoiceProcessor.Domain.DTOs.Requests.Auth;

namespace VoiceProcessor.Managers.Tests.Validators;

public class ResetPasswordRequestValidatorTests
{
    private readonly ResetPasswordRequestValidator _validator;

    public ResetPasswordRequestValidatorTests()
    {
        _validator = new ResetPasswordRequestValidator();
    }

    [Fact]
    public async Task Validate_WithEmptyToken_ShouldFail()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = string.Empty,
            NewPassword = "ValidPassword123"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Token");
    }

    [Fact]
    public async Task Validate_WithNullToken_ShouldFail()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = null!,
            NewPassword = "ValidPassword123"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Token");
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_ShouldFail()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "valid-token-123",
            NewPassword = string.Empty
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public async Task Validate_WithNullPassword_ShouldFail()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "valid-token-123",
            NewPassword = null!
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public async Task Validate_WithPasswordShorterThan8Characters_ShouldFail()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "valid-token-123",
            NewPassword = "Short1"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public async Task Validate_WithPasswordExactly7Characters_ShouldFail()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "valid-token-123",
            NewPassword = "Pass123"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public async Task Validate_WithPasswordExactly8Characters_ShouldSucceed()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "valid-token-123",
            NewPassword = "Pass1234"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "valid-reset-token-abc123xyz",
            NewPassword = "NewSecurePassword123"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithLongPassword_ShouldSucceed()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "valid-token-123",
            NewPassword = new string('a', 100)
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
