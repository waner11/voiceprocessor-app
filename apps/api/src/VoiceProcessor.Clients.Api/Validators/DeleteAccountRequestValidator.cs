using FluentValidation;
using VoiceProcessor.Domain.DTOs.Requests.Auth;

namespace VoiceProcessor.Clients.Api.Validators;

public class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountRequestValidator()
    {
        // Password field is optional - no validation needed
        // Manager handles validation logic
    }
}
