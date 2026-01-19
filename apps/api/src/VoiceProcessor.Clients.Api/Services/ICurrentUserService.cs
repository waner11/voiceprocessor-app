namespace VoiceProcessor.Clients.Api.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Tier { get; }
    string? Name { get; }
    string? AuthMethod { get; }
    bool IsAuthenticated { get; }
}
