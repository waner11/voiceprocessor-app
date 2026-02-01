using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Domain.DTOs.Requests.Payments;
using VoiceProcessor.Domain.DTOs.Responses.Payments;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Managers.Payment;

namespace VoiceProcessor.Managers.Tests.Payment;

public class PaymentManagerTests
{
    private readonly Mock<IStripeAccessor> _mockStripeAccessor;
    private readonly Mock<IPaymentHistoryAccessor> _mockPaymentHistoryAccessor;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly Mock<ILogger<PaymentManager>> _mockLogger;

    public PaymentManagerTests()
    {
        _mockStripeAccessor = new Mock<IStripeAccessor>();
        _mockPaymentHistoryAccessor = new Mock<IPaymentHistoryAccessor>();
        _mockUserAccessor = new Mock<IUserAccessor>();
        _mockLogger = new Mock<ILogger<PaymentManager>>();
    }

    private PaymentManager CreateManager()
    {
        return new PaymentManager(
            _mockStripeAccessor.Object,
            _mockPaymentHistoryAccessor.Object,
            _mockUserAccessor.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetCreditPacksAsync_ReturnsPacksFromStripeAccessor()
    {
        // Arrange
        var manager = CreateManager();
        var expectedPacks = new List<CreditPackResponse>
        {
            new CreditPackResponse
            {
                PriceId = "price_123",
                ProductId = "prod_123",
                Name = "Starter Pack",
                Credits = 1000,
                PriceAmount = 10.00m,
                Currency = "usd"
            },
            new CreditPackResponse
            {
                PriceId = "price_456",
                ProductId = "prod_456",
                Name = "Pro Pack",
                Credits = 5000,
                PriceAmount = 40.00m,
                Currency = "usd"
            }
        };

        _mockStripeAccessor.Setup(x => x.GetCreditPacksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPacks);

        // Act
        var result = await manager.GetCreditPacksAsync();

        // Assert
        result.Should().NotBeNull();
        result.Packs.Should().HaveCount(2);
        result.Packs.Should().BeEquivalentTo(expectedPacks);
        _mockStripeAccessor.Verify(x => x.GetCreditPacksAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCreditPacksAsync_PropagatesCancellation()
    {
        // Arrange
        var manager = CreateManager();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockStripeAccessor.Setup(x => x.GetCreditPacksAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = async () => await manager.GetCreditPacksAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task CreateCheckoutAsync_ValidRequest_ReturnsCheckoutResponse()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var request = new CreateCheckoutRequest
        {
            PriceId = "price_123",
            SuccessUrl = "https://example.com/success",
            CancelUrl = "https://example.com/cancel"
        };

        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            IsActive = true
        };

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockStripeAccessor.Setup(x => x.CreateCheckoutSessionAsync(
                userId,
                request.PriceId,
                request.SuccessUrl,
                request.CancelUrl,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StripeCheckoutResult
            {
                Success = true,
                SessionId = "cs_test_123",
                CheckoutUrl = "https://checkout.stripe.com/pay/cs_test_123"
            });

        // Act
        var result = await manager.CreateCheckoutAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.SessionId.Should().Be("cs_test_123");
        result.CheckoutUrl.Should().Be("https://checkout.stripe.com/pay/cs_test_123");
        _mockUserAccessor.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockStripeAccessor.Verify(x => x.CreateCheckoutSessionAsync(
            userId,
            request.PriceId,
            request.SuccessUrl,
            request.CancelUrl,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCheckoutAsync_UserNotFound_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var request = new CreateCheckoutRequest
        {
            PriceId = "price_123",
            SuccessUrl = "https://example.com/success",
            CancelUrl = "https://example.com/cancel"
        };

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.User?)null);

        // Act
        var act = async () => await manager.CreateCheckoutAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"User {userId} not found");
        _mockStripeAccessor.Verify(x => x.CreateCheckoutSessionAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateCheckoutAsync_StripeFailure_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var request = new CreateCheckoutRequest
        {
            PriceId = "price_123",
            SuccessUrl = "https://example.com/success",
            CancelUrl = "https://example.com/cancel"
        };

        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            IsActive = true
        };

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockStripeAccessor.Setup(x => x.CreateCheckoutSessionAsync(
                userId,
                request.PriceId,
                request.SuccessUrl,
                request.CancelUrl,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StripeCheckoutResult
            {
                Success = false,
                ErrorMessage = "Invalid price ID"
            });

        // Act
        var act = async () => await manager.CreateCheckoutAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid price ID");
    }

    [Fact]
    public async Task HandleWebhookAsync_ValidCheckoutSessionCompleted_ProcessesCorrectly()
    {
        // Arrange
        var manager = CreateManager();
        var payload = "{\"type\":\"checkout.session.completed\"}";
        var signature = "test_signature";
        var userId = Guid.NewGuid();
        var sessionId = "cs_test_123";

        var checkoutData = new StripeCheckoutData
        {
            SessionId = sessionId,
            PaymentIntentId = "pi_test_123",
            UserId = userId,
            PriceId = "price_123",
            Credits = 1000,
            PackName = "Starter Pack",
            Amount = 10.00m,
            Currency = "usd"
        };

        _mockStripeAccessor.Setup(x => x.ParseWebhook(payload, signature))
            .Returns(new StripeWebhookResult
            {
                Success = true,
                EventType = "checkout.session.completed",
                CheckoutData = checkoutData
            });

        _mockPaymentHistoryAccessor.Setup(x => x.GetByStripeSessionIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentHistory?)null);

        PaymentHistory? capturedPayment = null;
        _mockPaymentHistoryAccessor.Setup(x => x.CreateAsync(It.IsAny<PaymentHistory>(), It.IsAny<CancellationToken>()))
            .Callback<PaymentHistory, CancellationToken>((p, ct) => capturedPayment = p)
            .ReturnsAsync((PaymentHistory p, CancellationToken ct) => p);

        // Act
        await manager.HandleWebhookAsync(payload, signature);

        // Assert
        _mockUserAccessor.Verify(x => x.AddCreditsAsync(userId, 1000, It.IsAny<CancellationToken>()), Times.Once);
        _mockPaymentHistoryAccessor.Verify(x => x.CreateAsync(It.IsAny<PaymentHistory>(), It.IsAny<CancellationToken>()), Times.Once);

        capturedPayment.Should().NotBeNull();
        capturedPayment!.UserId.Should().Be(userId);
        capturedPayment.StripeSessionId.Should().Be(sessionId);
        capturedPayment.StripePaymentIntentId.Should().Be("pi_test_123");
        capturedPayment.Amount.Should().Be(10.00m);
        capturedPayment.Currency.Should().Be("usd");
        capturedPayment.CreditsAdded.Should().Be(1000);
        capturedPayment.PackId.Should().Be("price_123");
        capturedPayment.PackName.Should().Be("Starter Pack");
        capturedPayment.Status.Should().Be("completed");
    }

    [Fact]
    public async Task HandleWebhookAsync_InvalidWebhook_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        var payload = "{\"type\":\"invalid\"}";
        var signature = "invalid_signature";

        _mockStripeAccessor.Setup(x => x.ParseWebhook(payload, signature))
            .Returns(new StripeWebhookResult
            {
                Success = false,
                ErrorMessage = "Invalid signature"
            });

        // Act
        var act = async () => await manager.HandleWebhookAsync(payload, signature);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid signature");
        _mockUserAccessor.Verify(x => x.AddCreditsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockPaymentHistoryAccessor.Verify(x => x.CreateAsync(It.IsAny<PaymentHistory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleWebhookAsync_IdempotentReplay_SkipsProcessing()
    {
        // Arrange
        var manager = CreateManager();
        var payload = "{\"type\":\"checkout.session.completed\"}";
        var signature = "test_signature";
        var userId = Guid.NewGuid();
        var sessionId = "cs_test_123";

        var checkoutData = new StripeCheckoutData
        {
            SessionId = sessionId,
            PaymentIntentId = "pi_test_123",
            UserId = userId,
            PriceId = "price_123",
            Credits = 1000,
            PackName = "Starter Pack",
            Amount = 10.00m,
            Currency = "usd"
        };

        _mockStripeAccessor.Setup(x => x.ParseWebhook(payload, signature))
            .Returns(new StripeWebhookResult
            {
                Success = true,
                EventType = "checkout.session.completed",
                CheckoutData = checkoutData
            });

        var existingPayment = new PaymentHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StripeSessionId = sessionId,
            CreditsAdded = 1000,
            Amount = 10.00m,
            Currency = "usd",
            Status = "completed",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _mockPaymentHistoryAccessor.Setup(x => x.GetByStripeSessionIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        // Act
        await manager.HandleWebhookAsync(payload, signature);

        // Assert
        _mockUserAccessor.Verify(x => x.AddCreditsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockPaymentHistoryAccessor.Verify(x => x.CreateAsync(It.IsAny<PaymentHistory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleWebhookAsync_NonCheckoutEvent_IgnoresEvent()
    {
        // Arrange
        var manager = CreateManager();
        var payload = "{\"type\":\"payment_intent.succeeded\"}";
        var signature = "test_signature";

        _mockStripeAccessor.Setup(x => x.ParseWebhook(payload, signature))
            .Returns(new StripeWebhookResult
            {
                Success = true,
                EventType = "payment_intent.succeeded",
                CheckoutData = null
            });

        // Act
        await manager.HandleWebhookAsync(payload, signature);

        // Assert
        _mockUserAccessor.Verify(x => x.AddCreditsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockPaymentHistoryAccessor.Verify(x => x.CreateAsync(It.IsAny<PaymentHistory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetPaymentHistoryAsync_ReturnsCorrectlyMappedHistory()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();

        var payments = new List<PaymentHistory>
        {
            new PaymentHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StripeSessionId = "cs_test_1",
                Amount = 10.00m,
                Currency = "usd",
                CreditsAdded = 1000,
                PackName = "Starter Pack",
                Status = "completed",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new PaymentHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StripeSessionId = "cs_test_2",
                Amount = 40.00m,
                Currency = "usd",
                CreditsAdded = 5000,
                PackName = "Pro Pack",
                Status = "completed",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        _mockPaymentHistoryAccessor.Setup(x => x.GetByUserIdAsync(userId, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        // Act
        var result = await manager.GetPaymentHistoryAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Payments.Should().HaveCount(2);
        
        var firstPayment = result.Payments[0];
        firstPayment.Id.Should().Be(payments[0].Id);
        firstPayment.Amount.Should().Be(10.00m);
        firstPayment.Currency.Should().Be("usd");
        firstPayment.CreditsAdded.Should().Be(1000);
        firstPayment.PackName.Should().Be("Starter Pack");
        firstPayment.Status.Should().Be("completed");

        var secondPayment = result.Payments[1];
        secondPayment.Id.Should().Be(payments[1].Id);
        secondPayment.Amount.Should().Be(40.00m);
        secondPayment.Currency.Should().Be("usd");
        secondPayment.CreditsAdded.Should().Be(5000);
        secondPayment.PackName.Should().Be("Pro Pack");
        secondPayment.Status.Should().Be("completed");

        _mockPaymentHistoryAccessor.Verify(x => x.GetByUserIdAsync(userId, 50, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPaymentHistoryAsync_EmptyHistory_ReturnsEmptyList()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();

        _mockPaymentHistoryAccessor.Setup(x => x.GetByUserIdAsync(userId, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PaymentHistory>());

        // Act
        var result = await manager.GetPaymentHistoryAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Payments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPaymentHistoryAsync_PropagatesCancellation()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockPaymentHistoryAccessor.Setup(x => x.GetByUserIdAsync(userId, 50, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = async () => await manager.GetPaymentHistoryAsync(userId, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
