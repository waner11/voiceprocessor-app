using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Resend;
using VoiceProcessor.Accessors.Notifications;

namespace VoiceProcessor.Accessors.Tests.Notifications;

public class ResendEmailAccessorTests
{
    private readonly Mock<IResend> _resendMock;
    private readonly Mock<ILogger<ResendEmailAccessor>> _loggerMock;
    private readonly ResendOptions _options;

    public ResendEmailAccessorTests()
    {
        _resendMock = new Mock<IResend>();
        _loggerMock = new Mock<ILogger<ResendEmailAccessor>>();
        _options = new ResendOptions
        {
            ApiKey = "test-api-key",
            FromEmail = "noreply@test.com",
            FromName = "Test App"
        };
    }

    [Fact]
    public async Task SendEmailAsync_WithValidParameters_CallsResendWithCorrectMessage()
    {
        // Arrange
        EmailMessage? capturedMessage = null;
        _resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((msg, _) => capturedMessage = msg)
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), new ResendRateLimit()));

        var accessor = new ResendEmailAccessor(_resendMock.Object, Options.Create(_options), _loggerMock.Object);

        // Act
        await accessor.SendEmailAsync(
            to: "user@example.com",
            subject: "Reset your password",
            htmlBody: "<p>Click <a href='https://example.com/reset'>here</a> to reset.</p>",
            cancellationToken: CancellationToken.None);

        // Assert
        _resendMock.Verify(
            r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);

        capturedMessage.Should().NotBeNull();
        capturedMessage!.From.Should().Be("Test App <noreply@test.com>");
        capturedMessage.To.Should().Contain("user@example.com");
        capturedMessage.Subject.Should().Be("Reset your password");
        capturedMessage.HtmlBody.Should().Contain("Click");
    }

    [Fact]
    public async Task SendEmailAsync_WhenResendThrows_ThrowsInvalidOperationException()
    {
        // Arrange
        _resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Resend API error"));

        var accessor = new ResendEmailAccessor(_resendMock.Object, Options.Create(_options), _loggerMock.Object);

        // Act
        var act = async () => await accessor.SendEmailAsync(
            to: "user@example.com",
            subject: "Test",
            htmlBody: "<p>Test</p>",
            cancellationToken: CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Failed to send email*");
    }
}
