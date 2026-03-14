using FluentValidation;
using VoiceProcessor.Domain.DTOs.Requests.Auth;

namespace VoiceProcessor.Clients.Api.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MinimumLength(1)
            .WithMessage("Name must be at least 1 character long")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters");
    }
}
