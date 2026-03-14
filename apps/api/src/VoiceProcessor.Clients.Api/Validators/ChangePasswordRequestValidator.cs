using FluentValidation;
using VoiceProcessor.Domain.DTOs.Requests.Auth;

namespace VoiceProcessor.Clients.Api.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required")
            .MinimumLength(8)
            .WithMessage("New password must be at least 8 characters long");
    }
}
