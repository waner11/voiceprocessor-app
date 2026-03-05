namespace VoiceProcessor.Accessors.Notifications;

public class ResendOptions
{
    public const string SectionName = "Resend";

    public required string ApiKey { get; set; }
    public required string FromEmail { get; set; }
    public required string FromName { get; set; }
}
