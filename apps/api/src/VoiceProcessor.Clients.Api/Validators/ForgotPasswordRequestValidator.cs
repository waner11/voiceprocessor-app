using FluentValidation;
using VoiceProcessor.Domain.DTOs.Requests.Auth;

namespace VoiceProcessor.Clients.Api.Validators;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");
    }
}
