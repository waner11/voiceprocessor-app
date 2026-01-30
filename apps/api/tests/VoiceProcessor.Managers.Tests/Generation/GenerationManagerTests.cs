using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Providers;
using VoiceProcessor.Domain.DTOs.Requests;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Managers.Contracts;
using VoiceProcessor.Managers.Generation;

namespace VoiceProcessor.Managers.Tests.Generation;

public class GenerationManagerTests
{
    private readonly Mock<IGenerationAccessor> _mockGenerationAccessor;
    private readonly Mock<IVoiceAccessor> _mockVoiceAccessor;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly Mock<ITtsProviderFactory> _mockProviderFactory;
    private readonly Mock<IChunkingEngine> _mockChunkingEngine;
    private readonly Mock<IPricingEngine> _mockPricingEngine;
    private readonly Mock<IRoutingEngine> _mockRoutingEngine;
    private readonly Mock<IBackgroundJobClient> _mockJobClient;
    private readonly Mock<ILogger<GenerationManager>> _mockLogger;

    public GenerationManagerTests()
    {
        _mockGenerationAccessor = new Mock<IGenerationAccessor>();
        _mockVoiceAccessor = new Mock<IVoiceAccessor>();
        _mockUserAccessor = new Mock<IUserAccessor>();
        _mockProviderFactory = new Mock<ITtsProviderFactory>();
        _mockChunkingEngine = new Mock<IChunkingEngine>();
        _mockPricingEngine = new Mock<IPricingEngine>();
        _mockRoutingEngine = new Mock<IRoutingEngine>();
        _mockJobClient = new Mock<IBackgroundJobClient>();
        _mockLogger = new Mock<ILogger<GenerationManager>>();
    }

    private GenerationManager CreateManager()
    {
        return new GenerationManager(
            _mockGenerationAccessor.Object,
            _mockVoiceAccessor.Object,
            _mockUserAccessor.Object,
            _mockProviderFactory.Object,
            _mockChunkingEngine.Object,
            _mockPricingEngine.Object,
            _mockRoutingEngine.Object,
            _mockJobClient.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task EstimateCostAsync_ValidInput_ReturnsCostEstimate()
    {
        // Arrange
        var manager = CreateManager();
        var voiceId = Guid.NewGuid();
        var request = new EstimateCostRequest
        {
            Text = "This is a test text for cost estimation.",
            VoiceId = voiceId,
            Provider = Provider.ElevenLabs,
            RoutingPreference = RoutingPreference.Balanced
        };

        var voice = new Domain.Entities.Voice
        {
            Id = voiceId,
            Name = "Test Voice",
            Provider = Provider.ElevenLabs,
            ProviderVoiceId = "voice_123",
            CostPerThousandChars = 0.30m,
            IsActive = true
        };

        _mockVoiceAccessor.Setup(x => x.GetByIdAsync(voiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(voice);

        _mockChunkingEngine.Setup(x => x.EstimateChunkCount(request.Text, null))
            .Returns(1);

        var providerEstimates = new List<ProviderPriceEstimate>
        {
            new ProviderPriceEstimate
            {
                Provider = Provider.ElevenLabs,
                CostPerThousandChars = 0.30m,
                TotalCost = 0.012m,
                Currency = "USD",
                CreditsRequired = 12
            },
            new ProviderPriceEstimate
            {
                Provider = Provider.OpenAI,
                CostPerThousandChars = 0.015m,
                TotalCost = 0.0006m,
                Currency = "USD",
                CreditsRequired = 1
            }
        };

        _mockPricingEngine.Setup(x => x.CalculateAllProviderEstimates(It.IsAny<PricingContext>()))
            .Returns(providerEstimates);

        var mockProvider = new Mock<ITtsProviderAccessor>();
        mockProvider.Setup(x => x.Provider).Returns(Provider.ElevenLabs);

        _mockProviderFactory.Setup(x => x.GetAllProviders())
            .Returns(new List<ITtsProviderAccessor> { mockProvider.Object });

        _mockRoutingEngine.Setup(x => x.SelectProviderAsync(It.IsAny<RoutingContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoutingDecision
            {
                SelectedProvider = Provider.ElevenLabs,
                Reason = "Best quality for balanced preference",
                EstimatedCost = 0.012m,
                EstimatedLatencyMs = 2000
            });

        // Act
        var result = await manager.EstimateCostAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CharacterCount.Should().Be(request.Text.Length);
        result.EstimatedChunks.Should().Be(1);
        result.EstimatedCost.Should().Be(0.012m);
        result.CreditsRequired.Should().Be(12);
        result.Currency.Should().Be("USD");
        result.RecommendedProvider.Should().Be(Provider.ElevenLabs);
        result.ProviderEstimates.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateGenerationAsync_ValidRequest_CreatesAndEnqueuesJob()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var voiceId = Guid.NewGuid();

        var request = new CreateGenerationRequest
        {
            Text = "This is a test generation request.",
            VoiceId = voiceId,
            RoutingPreference = RoutingPreference.Balanced,
            AudioFormat = "mp3"
        };

        var voice = new Domain.Entities.Voice
        {
            Id = voiceId,
            Name = "Test Voice",
            Provider = Provider.ElevenLabs,
            ProviderVoiceId = "voice_123",
            CostPerThousandChars = 0.30m,
            IsActive = true
        };

        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            CreditsRemaining = 1000,
            IsActive = true
        };

        _mockVoiceAccessor.Setup(x => x.GetByIdAsync(voiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(voice);

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPricingEngine.Setup(x => x.CalculateEstimate(It.IsAny<PricingContext>()))
            .Returns(new PriceEstimate
            {
                CharacterCount = request.Text.Length,
                EstimatedCost = 0.01m,
                Currency = "USD",
                Provider = Provider.ElevenLabs,
                CreditsRequired = 10
            });

        _mockChunkingEngine.Setup(x => x.SplitText(request.Text, null))
            .Returns(new List<TextChunk>
            {
                new TextChunk { Index = 0, Text = request.Text, StartPosition = 0, EndPosition = request.Text.Length }
            });

        var mockProvider = new Mock<ITtsProviderAccessor>();
        mockProvider.Setup(x => x.Provider).Returns(Provider.ElevenLabs);

        _mockProviderFactory.Setup(x => x.GetAllProviders())
            .Returns(new List<ITtsProviderAccessor> { mockProvider.Object });

        _mockRoutingEngine.Setup(x => x.SelectProviderAsync(It.IsAny<RoutingContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoutingDecision
            {
                SelectedProvider = Provider.ElevenLabs,
                Reason = "Best match",
                EstimatedCost = 0.01m,
                EstimatedLatencyMs = 2000
            });

        Domain.Entities.Generation? capturedGeneration = null;
        _mockGenerationAccessor.Setup(x => x.CreateAsync(It.IsAny<Domain.Entities.Generation>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Generation, CancellationToken>((g, ct) => capturedGeneration = g)
            .ReturnsAsync((Domain.Entities.Generation g, CancellationToken ct) => g);

        // Act
        var result = await manager.CreateGenerationAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(GenerationStatus.Pending);
        result.CharacterCount.Should().Be(request.Text.Length);
        result.Provider.Should().Be(Provider.ElevenLabs);

        capturedGeneration.Should().NotBeNull();
        capturedGeneration!.UserId.Should().Be(userId);
        capturedGeneration.VoiceId.Should().Be(voiceId);
        capturedGeneration.InputText.Should().Be(request.Text);
        capturedGeneration.Status.Should().Be(GenerationStatus.Pending);

        _mockGenerationAccessor.Verify(x => x.CreateAsync(It.IsAny<Domain.Entities.Generation>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockJobClient.Verify(x => x.Create(It.IsAny<Hangfire.Common.Job>(), It.IsAny<Hangfire.States.IState>()), Times.Once);
    }

    [Fact]
    public async Task CreateGenerationAsync_VoiceNotFound_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var voiceId = Guid.NewGuid();

        var request = new CreateGenerationRequest
        {
            Text = "Test text",
            VoiceId = voiceId,
            RoutingPreference = RoutingPreference.Balanced
        };

        _mockVoiceAccessor.Setup(x => x.GetByIdAsync(voiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Voice?)null);

        // Act
        var act = async () => await manager.CreateGenerationAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Voice {voiceId} not found");

        _mockGenerationAccessor.Verify(x => x.CreateAsync(It.IsAny<Domain.Entities.Generation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateGenerationAsync_InsufficientCredits_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        var userId = Guid.NewGuid();
        var voiceId = Guid.NewGuid();

        var request = new CreateGenerationRequest
        {
            Text = "Test text",
            VoiceId = voiceId,
            RoutingPreference = RoutingPreference.Balanced
        };

        var voice = new Domain.Entities.Voice
        {
            Id = voiceId,
            Name = "Test Voice",
            Provider = Provider.ElevenLabs,
            CostPerThousandChars = 0.30m,
            IsActive = true
        };

        var user = new Domain.Entities.User
        {
            Id = userId,
            Email = "test@example.com",
            CreditsRemaining = 5,
            IsActive = true
        };

        _mockVoiceAccessor.Setup(x => x.GetByIdAsync(voiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(voice);

        _mockUserAccessor.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPricingEngine.Setup(x => x.CalculateEstimate(It.IsAny<PricingContext>()))
            .Returns(new PriceEstimate
            {
                CharacterCount = request.Text.Length,
                EstimatedCost = 0.10m,
                Currency = "USD",
                Provider = Provider.ElevenLabs,
                CreditsRequired = 100
            });

        // Act
        var act = async () => await manager.CreateGenerationAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Insufficient credits");

        _mockGenerationAccessor.Verify(x => x.CreateAsync(It.IsAny<Domain.Entities.Generation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetGenerationAsync_ExistingId_ReturnsData()
    {
        // Arrange
        var manager = CreateManager();
        var generationId = Guid.NewGuid();

        var generation = new Domain.Entities.Generation
        {
            Id = generationId,
            UserId = Guid.NewGuid(),
            VoiceId = Guid.NewGuid(),
            InputText = "Test text",
            CharacterCount = 9,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            AudioUrl = "https://storage.example.com/audio.mp3",
            AudioFormat = "mp3",
            AudioDurationMs = 5000,
            EstimatedCost = 0.01m,
            ActualCost = 0.009m,
            ChunkCount = 1,
            ChunksCompleted = 1,
            Progress = 100,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            StartedAt = DateTime.UtcNow.AddMinutes(-9),
            CompletedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);

        // Act
        var result = await manager.GetGenerationAsync(generationId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(generationId);
        result.Status.Should().Be(GenerationStatus.Completed);
        result.CharacterCount.Should().Be(9);
        result.AudioUrl.Should().Be("https://storage.example.com/audio.mp3");
        result.Progress.Should().Be(100);
    }

    [Fact]
    public async Task GetGenerationAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        var manager = CreateManager();
        var generationId = Guid.NewGuid();

        _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Generation?)null);

        // Act
        var result = await manager.GetGenerationAsync(generationId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CancelGenerationAsync_PendingGeneration_CancelsSuccessfully()
    {
        // Arrange
        var manager = CreateManager();
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var generation = new Domain.Entities.Generation
        {
            Id = generationId,
            UserId = userId,
            VoiceId = Guid.NewGuid(),
            InputText = "Test",
            CharacterCount = 4,
            Status = GenerationStatus.Pending,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            ChunkCount = 1,
            ChunksCompleted = 0,
            Progress = 0,
            CreatedAt = DateTime.UtcNow
        };

        _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);

        // Act
        var result = await manager.CancelGenerationAsync(generationId, userId);

        // Assert
        result.Should().BeTrue();
        _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(generationId, GenerationStatus.Cancelled, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelGenerationAsync_CompletedGeneration_ReturnsFalse()
    {
        // Arrange
        var manager = CreateManager();
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var generation = new Domain.Entities.Generation
        {
            Id = generationId,
            UserId = userId,
            VoiceId = Guid.NewGuid(),
            InputText = "Test",
            CharacterCount = 4,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            ChunkCount = 1,
            ChunksCompleted = 1,
            Progress = 100,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);

        // Act
        var result = await manager.CancelGenerationAsync(generationId, userId);

        // Assert
        result.Should().BeFalse();
        _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<GenerationStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelGenerationAsync_OtherUsersGeneration_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var generation = new Domain.Entities.Generation
        {
            Id = generationId,
            UserId = otherUserId,
            VoiceId = Guid.NewGuid(),
            InputText = "Test",
            CharacterCount = 4,
            Status = GenerationStatus.Pending,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            ChunkCount = 1,
            ChunksCompleted = 0,
            Progress = 0,
            CreatedAt = DateTime.UtcNow
        };

        _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);

        // Act
        var act = async () => await manager.CancelGenerationAsync(generationId, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Generation does not belong to user");

        _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<GenerationStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_OtherUsersGeneration_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var request = new SubmitFeedbackRequest
        {
            Rating = 5,
            Comment = "Great quality!"
        };

        var generation = new Domain.Entities.Generation
        {
            Id = generationId,
            UserId = otherUserId,
            VoiceId = Guid.NewGuid(),
            InputText = "Test",
            CharacterCount = 4,
            Status = GenerationStatus.Completed,
            RoutingPreference = RoutingPreference.Balanced,
            SelectedProvider = Provider.ElevenLabs,
            ChunkCount = 1,
            ChunksCompleted = 1,
            Progress = 100,
            CreatedAt = DateTime.UtcNow
        };

        _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);

        // Act
        var act = async () => await manager.SubmitFeedbackAsync(generationId, userId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Generation does not belong to user");
    }

    [Fact]
    public async Task EstimateCostAsync_RoutingFailure_GracefullyDegrades()
    {
        // Arrange
        var manager = CreateManager();
        var voiceId = Guid.NewGuid();
        var request = new EstimateCostRequest
        {
            Text = "This is a test text for cost estimation.",
            VoiceId = voiceId,
            Provider = Provider.ElevenLabs,
            RoutingPreference = RoutingPreference.Balanced
        };

        var voice = new Domain.Entities.Voice
        {
            Id = voiceId,
            Name = "Test Voice",
            Provider = Provider.ElevenLabs,
            ProviderVoiceId = "voice_123",
            CostPerThousandChars = 0.30m,
            IsActive = true
        };

        _mockVoiceAccessor.Setup(x => x.GetByIdAsync(voiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(voice);

        _mockChunkingEngine.Setup(x => x.EstimateChunkCount(request.Text, null))
            .Returns(1);

        var providerEstimates = new List<ProviderPriceEstimate>
        {
            new ProviderPriceEstimate
            {
                Provider = Provider.ElevenLabs,
                CostPerThousandChars = 0.30m,
                TotalCost = 0.012m,
                Currency = "USD",
                CreditsRequired = 12
            }
        };

        _mockPricingEngine.Setup(x => x.CalculateAllProviderEstimates(It.IsAny<PricingContext>()))
            .Returns(providerEstimates);

        var mockProvider = new Mock<ITtsProviderAccessor>();
        mockProvider.Setup(x => x.Provider).Returns(Provider.ElevenLabs);

        _mockProviderFactory.Setup(x => x.GetAllProviders())
            .Returns(new List<ITtsProviderAccessor> { mockProvider.Object });

        _mockRoutingEngine.Setup(x => x.SelectProviderAsync(It.IsAny<RoutingContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Routing service unavailable"));

        // Act
        var result = await manager.EstimateCostAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CharacterCount.Should().Be(request.Text.Length);
        result.EstimatedChunks.Should().Be(1);
        result.EstimatedCost.Should().Be(0.012m);
        result.RecommendedProvider.Should().BeNull();
        result.ProviderEstimates.Should().HaveCount(1);
    }
}
