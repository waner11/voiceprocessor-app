using FluentValidation;
using VoiceProcessor.Domain.DTOs.Requests;

namespace VoiceProcessor.Clients.Api.Validators;

public class CreateGenerationRequestValidator : AbstractValidator<CreateGenerationRequest>
{
    public CreateGenerationRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text is required")
            .MaximumLength(500_000)
            .WithMessage("Text cannot exceed 500,000 characters");
    }
}
