using FluentAssertions;
using FluentValidation;
using VoiceProcessor.Clients.Api.Validators;
using VoiceProcessor.Domain.DTOs.Requests.Auth;

namespace VoiceProcessor.Managers.Tests.Validators;

public class ForgotPasswordRequestValidatorTests
{
    private readonly ForgotPasswordRequestValidator _validator;

    public ForgotPasswordRequestValidatorTests()
    {
        _validator = new ForgotPasswordRequestValidator();
    }

    [Fact]
    public async Task Validate_WithEmptyEmail_ShouldFail()
    {
        // Arrange
        var request = new ForgotPasswordRequest
        {
            Email = string.Empty
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_WithNullEmail_ShouldFail()
    {
        // Arrange
        var request = new ForgotPasswordRequest
        {
            Email = null!
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldFail()
    {
        // Arrange
        var request = new ForgotPasswordRequest
        {
            Email = "not-an-email"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_WithValidEmail_ShouldSucceed()
    {
        // Arrange
        var request = new ForgotPasswordRequest
        {
            Email = "user@example.com"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithValidEmailVariations_ShouldSucceed()
    {
        // Arrange
        var validEmails = new[]
        {
            "test@domain.com",
            "user.name@example.co.uk",
            "first+last@test.org"
        };

        // Act & Assert
        foreach (var email in validEmails)
        {
            var request = new ForgotPasswordRequest { Email = email };
            var result = await _validator.ValidateAsync(request);
            result.IsValid.Should().BeTrue($"Email {email} should be valid");
        }
    }
}
