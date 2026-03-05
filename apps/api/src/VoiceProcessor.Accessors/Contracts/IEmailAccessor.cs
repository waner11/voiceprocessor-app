namespace VoiceProcessor.Accessors.Contracts;

public interface IEmailAccessor
{
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
