using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;
using VoiceProcessor.Accessors.Contracts;

namespace VoiceProcessor.Accessors.Notifications;

public class ResendEmailAccessor : IEmailAccessor
{
    private readonly IResend _resend;
    private readonly ResendOptions _options;
    private readonly ILogger<ResendEmailAccessor> _logger;

    public ResendEmailAccessor(
        IResend resend,
        IOptions<ResendOptions> options,
        ILogger<ResendEmailAccessor> logger)
    {
        _resend = resend;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending email to {Recipient} with subject {Subject}", to, subject);

        try
        {
            var message = new EmailMessage
            {
                From = $"{_options.FromName} <{_options.FromEmail}>",
                Subject = subject,
                HtmlBody = htmlBody
            };
            message.To.Add(to);

            await _resend.EmailSendAsync(message, cancellationToken);

            _logger.LogInformation("Email sent successfully to {Recipient}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient} with subject {Subject}", to, subject);
            throw new InvalidOperationException($"Failed to send email to {to}: {ex.Message}", ex);
        }
    }
}
