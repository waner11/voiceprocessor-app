using System.ComponentModel.DataAnnotations;

namespace VoiceProcessor.Domain.DTOs.Requests.Auth;

public class OAuthLoginRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string RedirectUri { get; set; } = string.Empty;

    public string? State { get; set; }
}
