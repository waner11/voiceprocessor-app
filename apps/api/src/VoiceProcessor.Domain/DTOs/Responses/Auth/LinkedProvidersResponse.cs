namespace VoiceProcessor.Domain.DTOs.Responses.Auth;

public class LinkedProvidersResponse
{
    public required IReadOnlyList<LinkedProviderInfo> Providers { get; init; }
}

public class LinkedProviderInfo
{
    public required string Provider { get; init; }
    public required string Email { get; init; }
    public string? Name { get; init; }
    public required DateTime LinkedAt { get; init; }
}
