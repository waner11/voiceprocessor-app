using FluentAssertions;
using FluentValidation;
using VoiceProcessor.Clients.Api.Validators;
using VoiceProcessor.Domain.DTOs.Requests;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Managers.Tests.Validators;

public class CreateGenerationRequestValidatorTests
{
    private readonly CreateGenerationRequestValidator _validator;

    public CreateGenerationRequestValidatorTests()
    {
        _validator = new CreateGenerationRequestValidator();
    }

    [Fact]
    public async Task Validate_WithEmptyText_ShouldFail()
    {
        // Arrange
        var request = new CreateGenerationRequest
        {
            Text = string.Empty,
            VoiceId = Guid.NewGuid(),
            RoutingPreference = RoutingPreference.Balanced
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Text");
    }

    [Fact]
    public async Task Validate_WithNullText_ShouldFail()
    {
        // Arrange
        var request = new CreateGenerationRequest
        {
            Text = null!,
            VoiceId = Guid.NewGuid(),
            RoutingPreference = RoutingPreference.Balanced
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Text");
    }

    [Fact]
    public async Task Validate_WithTextExceeding500000Characters_ShouldFail()
    {
        // Arrange
        var request = new CreateGenerationRequest
        {
            Text = new string('a', 500_001),
            VoiceId = Guid.NewGuid(),
            RoutingPreference = RoutingPreference.Balanced
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Text");
    }

    [Fact]
    public async Task Validate_WithValidText_ShouldSucceed()
    {
        // Arrange
        var request = new CreateGenerationRequest
        {
            Text = "This is a valid text for text-to-speech generation.",
            VoiceId = Guid.NewGuid(),
            RoutingPreference = RoutingPreference.Balanced
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithSingleCharacter_ShouldSucceed()
    {
        // Arrange
        var request = new CreateGenerationRequest
        {
            Text = "a",
            VoiceId = Guid.NewGuid(),
            RoutingPreference = RoutingPreference.Balanced
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithMaximumAllowedLength_ShouldSucceed()
    {
        // Arrange
        var request = new CreateGenerationRequest
        {
            Text = new string('a', 500_000),
            VoiceId = Guid.NewGuid(),
            RoutingPreference = RoutingPreference.Balanced
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
