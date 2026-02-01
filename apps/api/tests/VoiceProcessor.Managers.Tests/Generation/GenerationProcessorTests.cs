using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Providers;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Managers.Generation;
using VoiceProcessor.Utilities.Timing;

namespace VoiceProcessor.Managers.Tests.Generation;

public class GenerationProcessorTests
{
    private readonly Mock<IGenerationAccessor> _mockGenerationAccessor;
    private readonly Mock<IGenerationChunkAccessor> _mockChunkAccessor;
    private readonly Mock<IVoiceAccessor> _mockVoiceAccessor;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly Mock<ITtsProviderFactory> _mockProviderFactory;
    private readonly Mock<IStorageAccessor> _mockStorageAccessor;
    private readonly Mock<IChunkingEngine> _mockChunkingEngine;
    private readonly Mock<IAudioMergeEngine> _mockAudioMergeEngine;
    private readonly Mock<ILogger<GenerationProcessor>> _mockLogger;
    private readonly Mock<IDelayService> _mockDelayService;

    public GenerationProcessorTests()
    {
       _mockGenerationAccessor = new Mock<IGenerationAccessor>();
       _mockChunkAccessor = new Mock<IGenerationChunkAccessor>();
       _mockVoiceAccessor = new Mock<IVoiceAccessor>();
       _mockUserAccessor = new Mock<IUserAccessor>();
       _mockProviderFactory = new Mock<ITtsProviderFactory>();
       _mockStorageAccessor = new Mock<IStorageAccessor>();
       _mockChunkingEngine = new Mock<IChunkingEngine>();
       _mockAudioMergeEngine = new Mock<IAudioMergeEngine>();
       _mockLogger = new Mock<ILogger<GenerationProcessor>>();
       _mockDelayService = new Mock<IDelayService>();
    }

    private GenerationProcessor CreateProcessor()
    {
       return new GenerationProcessor(
           _mockGenerationAccessor.Object,
           _mockChunkAccessor.Object,
           _mockVoiceAccessor.Object,
           _mockUserAccessor.Object,
           _mockProviderFactory.Object,
           _mockStorageAccessor.Object,
           _mockChunkingEngine.Object,
           _mockAudioMergeEngine.Object,
           _mockLogger.Object,
           _mockDelayService.Object
       );
    }

    private Domain.Entities.Generation CreateGeneration(GenerationStatus status = GenerationStatus.Pending)
    {
       return new Domain.Entities.Generation
       {
           Id = Guid.NewGuid(),
           UserId = Guid.NewGuid(),
           VoiceId = Guid.NewGuid(),
           InputText = "Test text for generation",
           CharacterCount = 24,
           Status = status,
           RoutingPreference = RoutingPreference.Balanced,
           SelectedProvider = Provider.ElevenLabs,
           AudioFormat = "mp3",
           ChunkCount = 1,
           ChunksCompleted = 0,
           Progress = 0,
           CreatedAt = DateTime.UtcNow
       };
    }

    private Domain.Entities.Voice CreateVoice(Guid? id = null)
    {
       return new Domain.Entities.Voice
       {
           Id = id ?? Guid.NewGuid(),
           Name = "Test Voice",
           Provider = Provider.ElevenLabs,
           ProviderVoiceId = "voice_123",
           CostPerThousandChars = 0.30m,
           IsActive = true
       };
    }

    private TtsResult CreateSuccessfulTtsResult()
    {
       return new TtsResult
       {
           Success = true,
           AudioData = new byte[] { 1, 2, 3, 4, 5 },
           ContentType = "audio/mpeg",
           DurationMs = 1000,
           Cost = 0.01m,
           CharactersProcessed = 24
       };
    }

    private void SetupChunkAccessorDefaults()
    {
       _mockChunkAccessor.Setup(x => x.CreateAsync(It.IsAny<GenerationChunk>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync((GenerationChunk chunk, CancellationToken ct) => chunk);
       _mockChunkAccessor.Setup(x => x.UpdateAsync(It.IsAny<GenerationChunk>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);
    }

    private Mock<ITtsProviderAccessor> SetupMockProvider(Provider provider = Provider.ElevenLabs)
    {
       var mockProvider = new Mock<ITtsProviderAccessor>();
       mockProvider.Setup(x => x.Provider).Returns(provider);
       _mockProviderFactory.Setup(x => x.GetProvider(provider))
           .Returns(mockProvider.Object);
       return mockProvider;
    }

    private void SetupCompletionPipeline(
        AudioMergeResult mergeResult,
        string storageUrl = "https://storage.example.com/chunk.mp3")
    {
        _mockStorageAccessor.Setup(x => x.UploadAsync(It.IsAny<StorageUploadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storageUrl);
        _mockAudioMergeEngine.Setup(x => x.MergeAudioChunksAsync(
            It.IsAny<IReadOnlyList<byte[]>>(),
            It.IsAny<AudioMergeOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mergeResult);
        _mockUserAccessor.Setup(x => x.TryDeductCreditsAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    // STATE MACHINE TESTS (4 tests)

    [Fact]
    public async Task ProcessGenerationAsync_GenerationNotFound_ReturnsSilently()
    {
       // Arrange
       var generationId = Guid.NewGuid();
       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generationId, It.IsAny<CancellationToken>()))
           .ReturnsAsync((Domain.Entities.Generation?)null);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generationId);

       // Assert
       _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(
           It.IsAny<Guid>(),
           It.IsAny<GenerationStatus>(),
           It.IsAny<string?>(),
           It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessGenerationAsync_CompletedGeneration_SkipsProcessing()
    {
       // Arrange
       var generation = CreateGeneration(GenerationStatus.Completed);
       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       _mockVoiceAccessor.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
       _mockChunkingEngine.Verify(x => x.SplitText(It.IsAny<string>(), It.IsAny<ChunkingOptions?>()), Times.Never);
    }

    [Fact]
    public async Task ProcessGenerationAsync_FailedGeneration_SkipsProcessing()
    {
       // Arrange
       var generation = CreateGeneration(GenerationStatus.Failed);
       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       _mockVoiceAccessor.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
       _mockChunkingEngine.Verify(x => x.SplitText(It.IsAny<string>(), It.IsAny<ChunkingOptions?>()), Times.Never);
    }

    [Fact]
    public async Task ProcessGenerationAsync_CancelledGeneration_SkipsProcessing()
    {
       // Arrange
       var generation = CreateGeneration(GenerationStatus.Cancelled);
       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       _mockVoiceAccessor.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
       _mockChunkingEngine.Verify(x => x.SplitText(It.IsAny<string>(), It.IsAny<ChunkingOptions?>()), Times.Never);
    }

    // HAPPY PATH TESTS (3 tests)

    [Fact]
    public async Task ProcessGenerationAsync_SingleChunk_CompletesSuccessfully()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);
       var ttsResult = CreateSuccessfulTtsResult();

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();
       mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(ttsResult);

       SetupChunkAccessorDefaults();

       var mergeResult = new AudioMergeResult
       {
           AudioData = new byte[] { 1, 2, 3, 4, 5 },
           ContentType = "audio/mpeg",
           DurationMs = 1000,
           SizeBytes = 5
       };
       SetupCompletionPipeline(mergeResult);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(
           generation.Id,
           GenerationStatus.Processing,
           null,
           It.IsAny<CancellationToken>()), Times.Once);
        _mockGenerationAccessor.Verify(x => x.SetCompletedAsync(
            generation.Id,
            It.IsAny<string>(),
            "mp3",
            1000,
            5,
            0.01m,
            It.IsAny<CancellationToken>()), Times.Once);
        _mockUserAccessor.Verify(x => x.TryDeductCreditsAsync(
            generation.UserId, 1, It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessGenerationAsync_MultipleChunks_ProcessesAllAndMerges()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);
       var ttsResult = CreateSuccessfulTtsResult();

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = "Chunk 1", StartPosition = 0, EndPosition = 7 },
           new TextChunk { Index = 1, Text = "Chunk 2", StartPosition = 7, EndPosition = 14 },
           new TextChunk { Index = 2, Text = "Chunk 3", StartPosition = 14, EndPosition = 21 }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();
       mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(ttsResult);

       SetupChunkAccessorDefaults();

       var mergeResult = new AudioMergeResult
       {
           AudioData = new byte[] { 1, 2, 3, 4, 5 },
           ContentType = "audio/mpeg",
           DurationMs = 3000,
           SizeBytes = 15
       };
       SetupCompletionPipeline(mergeResult);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       _mockChunkAccessor.Verify(x => x.CreateAsync(It.IsAny<GenerationChunk>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
       _mockChunkAccessor.Verify(x => x.UpdateAsync(It.Is<GenerationChunk>(c => c.Status == ChunkStatus.Completed), It.IsAny<CancellationToken>()), Times.Exactly(3));
       mockProvider.Verify(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
       _mockAudioMergeEngine.Verify(x => x.MergeAudioChunksAsync(
           It.Is<IReadOnlyList<byte[]>>(list => list.Count == 3),
           It.IsAny<AudioMergeOptions>(),
           It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessGenerationAsync_Completion_SetsCorrectMetadata()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);
       var ttsResult = new TtsResult
       {
           Success = true,
           AudioData = new byte[] { 1, 2, 3 },
           ContentType = "audio/mpeg",
           DurationMs = 2500,
           Cost = 0.05m,
           CharactersProcessed = 24
       };

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();
       mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(ttsResult);

       SetupChunkAccessorDefaults();

       var mergeResult = new AudioMergeResult
       {
           AudioData = new byte[] { 1, 2, 3 },
           ContentType = "audio/mpeg",
           DurationMs = 2500,
           SizeBytes = 12345
       };
       SetupCompletionPipeline(mergeResult, "https://storage.example.com/audio.mp3");

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
        _mockGenerationAccessor.Verify(x => x.SetCompletedAsync(
            generation.Id,
            "https://storage.example.com/audio.mp3",
            "mp3",
            2500,
            12345,
            0.05m,
            It.IsAny<CancellationToken>()), Times.Once);
        _mockUserAccessor.Verify(x => x.TryDeductCreditsAsync(
            generation.UserId, 5, It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // RETRY LOGIC TESTS (5 tests)

    [Fact]
    public async Task ProcessGenerationAsync_ChunkFailsOnce_RetriesAndSucceeds()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);
       var ttsResult = CreateSuccessfulTtsResult();

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();
       var callCount = 0;
       mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(() =>
           {
               callCount++;
               if (callCount == 1)
                   throw new InvalidOperationException("TTS provider temporarily unavailable");
               return ttsResult;
           });

       SetupChunkAccessorDefaults();

       var mergeResult = new AudioMergeResult
       {
           AudioData = new byte[] { 1, 2, 3 },
           ContentType = "audio/mpeg",
           DurationMs = 1000,
           SizeBytes = 5
       };
       SetupCompletionPipeline(mergeResult);

       _mockDelayService.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       mockProvider.Verify(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
       _mockDelayService.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(1), It.IsAny<CancellationToken>()), Times.Once);
       _mockGenerationAccessor.Verify(x => x.SetCompletedAsync(
           It.IsAny<Guid>(),
           It.IsAny<string>(),
           It.IsAny<string>(),
           It.IsAny<int>(),
           It.IsAny<long>(),
           It.IsAny<decimal>(),
           It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessGenerationAsync_ChunkFailsTwice_RetriesAndSucceeds()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);
       var ttsResult = CreateSuccessfulTtsResult();

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();
       var callCount = 0;
       mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(() =>
           {
               callCount++;
               if (callCount <= 2)
                   throw new InvalidOperationException("TTS provider temporarily unavailable");
               return ttsResult;
           });

       SetupChunkAccessorDefaults();

       var mergeResult = new AudioMergeResult
       {
           AudioData = new byte[] { 1, 2, 3 },
           ContentType = "audio/mpeg",
           DurationMs = 1000,
           SizeBytes = 5
       };
       SetupCompletionPipeline(mergeResult);

       _mockDelayService.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       mockProvider.Verify(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
       _mockDelayService.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(1), It.IsAny<CancellationToken>()), Times.Once);
       _mockDelayService.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(3), It.IsAny<CancellationToken>()), Times.Once);
       _mockGenerationAccessor.Verify(x => x.SetCompletedAsync(
           It.IsAny<Guid>(),
           It.IsAny<string>(),
           It.IsAny<string>(),
           It.IsAny<int>(),
           It.IsAny<long>(),
           It.IsAny<decimal>(),
           It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessGenerationAsync_ChunkFailsThrice_RetriesAndSucceeds()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);
       var ttsResult = CreateSuccessfulTtsResult();

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();
       var callCount = 0;
       mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(() =>
           {
               callCount++;
               if (callCount <= 3)
                   throw new InvalidOperationException("TTS provider temporarily unavailable");
               return ttsResult;
           });

       SetupChunkAccessorDefaults();

       var mergeResult = new AudioMergeResult
       {
           AudioData = new byte[] { 1, 2, 3 },
           ContentType = "audio/mpeg",
           DurationMs = 1000,
           SizeBytes = 5
       };
       SetupCompletionPipeline(mergeResult);

       _mockDelayService.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       mockProvider.Verify(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
       _mockDelayService.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(1), It.IsAny<CancellationToken>()), Times.Once);
       _mockDelayService.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(3), It.IsAny<CancellationToken>()), Times.Once);
       _mockDelayService.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(10), It.IsAny<CancellationToken>()), Times.Once);
       _mockGenerationAccessor.Verify(x => x.SetCompletedAsync(
           It.IsAny<Guid>(),
           It.IsAny<string>(),
           It.IsAny<string>(),
           It.IsAny<int>(),
           It.IsAny<long>(),
           It.IsAny<decimal>(),
           It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessGenerationAsync_ChunkExhaustsAllRetries_MarksGenerationFailed()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();
       mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
           .ThrowsAsync(new InvalidOperationException("TTS provider permanently unavailable"));

       SetupChunkAccessorDefaults();

       _mockDelayService.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       mockProvider.Verify(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
       _mockChunkAccessor.Verify(x => x.UpdateAsync(
           It.Is<GenerationChunk>(c => c.Status == ChunkStatus.Failed && c.ErrorMessage != null),
           It.IsAny<CancellationToken>()), Times.Once);
       _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(
           generation.Id,
           GenerationStatus.Failed,
           It.Is<string>(msg => msg.Contains("permanently unavailable")),
           It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessGenerationAsync_RetryUsesExponentialBackoff_VerifiesDelays()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);
       var ttsResult = CreateSuccessfulTtsResult();

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();
       var callCount = 0;
       mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(() =>
           {
               callCount++;
               if (callCount <= 2)
                   throw new InvalidOperationException("TTS provider temporarily unavailable");
               return ttsResult;
           });

       SetupChunkAccessorDefaults();

       var mergeResult = new AudioMergeResult
       {
           AudioData = new byte[] { 1, 2, 3 },
           ContentType = "audio/mpeg",
           DurationMs = 1000,
           SizeBytes = 5
       };
       SetupCompletionPipeline(mergeResult);

       _mockDelayService.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       _mockDelayService.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(1), It.IsAny<CancellationToken>()), Times.Once);
       _mockDelayService.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(3), It.IsAny<CancellationToken>()), Times.Once);
       _mockDelayService.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(10), It.IsAny<CancellationToken>()), Times.Never);
    }

    // CANCELLATION TESTS (2 tests)

    [Fact]
    public async Task ProcessGenerationAsync_CancellationDuringTts_UpdatesStatusToCancelled()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();
       mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
           .ThrowsAsync(new OperationCanceledException());

       _mockChunkAccessor.Setup(x => x.CreateAsync(It.IsAny<GenerationChunk>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync((GenerationChunk chunk, CancellationToken ct) => chunk);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(
           generation.Id,
           GenerationStatus.Cancelled,
           null,
           It.IsAny<CancellationToken>()), Times.Once);
       _mockDelayService.Verify(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessGenerationAsync_CancellationBeforeChunkProcessing_UpdatesStatusToCancelled()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();

       var processor = CreateProcessor();
       var cts = new CancellationTokenSource();
       cts.Cancel();

       // Act
       await processor.ProcessGenerationAsync(generation.Id, cts.Token);

       // Assert
       _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(
           generation.Id,
           GenerationStatus.Cancelled,
           null,
           It.IsAny<CancellationToken>()), Times.Once);
       mockProvider.Verify(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ERROR HANDLING TESTS (4 tests)

    [Fact]
    public async Task ProcessGenerationAsync_VoiceNotFound_MarksGenerationFailed()
    {
       // Arrange
       var generation = CreateGeneration();

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync((Domain.Entities.Voice?)null);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(
           generation.Id,
           GenerationStatus.Failed,
           It.Is<string>(msg => msg.Contains("not found")),
           It.IsAny<CancellationToken>()), Times.Once);
       _mockChunkingEngine.Verify(x => x.SplitText(It.IsAny<string>(), It.IsAny<ChunkingOptions?>()), Times.Never);
    }

    [Fact]
    public async Task ProcessGenerationAsync_TtsReturnsUnsuccessfulResult_Retries()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();
       mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new TtsResult
           {
               Success = false,
               ErrorMessage = "TTS generation failed"
           });

       SetupChunkAccessorDefaults();

       _mockDelayService.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       mockProvider.Verify(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
       _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(
           generation.Id,
           GenerationStatus.Failed,
           It.IsAny<string>(),
           It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessGenerationAsync_AudioMergeFails_MarksGenerationFailed()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);
       var ttsResult = CreateSuccessfulTtsResult();

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();
       mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(ttsResult);

       SetupChunkAccessorDefaults();

       _mockStorageAccessor.Setup(x => x.UploadAsync(It.IsAny<StorageUploadRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync("https://storage.example.com/chunk.mp3");

       _mockAudioMergeEngine.Setup(x => x.MergeAudioChunksAsync(
           It.IsAny<IReadOnlyList<byte[]>>(),
           It.IsAny<AudioMergeOptions>(),
           It.IsAny<CancellationToken>()))
           .ThrowsAsync(new InvalidOperationException("Audio merge failed"));

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(
           generation.Id,
           GenerationStatus.Failed,
           It.Is<string>(msg => msg.Contains("Audio merge failed")),
           It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessGenerationAsync_StorageUploadFinalFails_MarksGenerationFailed()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);
       var ttsResult = CreateSuccessfulTtsResult();

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();
       mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(ttsResult);

       SetupChunkAccessorDefaults();

       var callCount = 0;
       _mockStorageAccessor.Setup(x => x.UploadAsync(It.IsAny<StorageUploadRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(() =>
           {
               callCount++;
               if (callCount == 1)
                   return "https://storage.example.com/chunk.mp3";
               throw new InvalidOperationException("Storage upload failed");
           });

       var mergeResult = new AudioMergeResult
       {
           AudioData = new byte[] { 1, 2, 3 },
           ContentType = "audio/mpeg",
           DurationMs = 1000,
           SizeBytes = 5
       };
       _mockAudioMergeEngine.Setup(x => x.MergeAudioChunksAsync(
           It.IsAny<IReadOnlyList<byte[]>>(),
           It.IsAny<AudioMergeOptions>(),
           It.IsAny<CancellationToken>()))
           .ReturnsAsync(mergeResult);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(
           generation.Id,
           GenerationStatus.Failed,
           It.Is<string>(msg => msg.Contains("Storage upload failed")),
           It.IsAny<CancellationToken>()), Times.Once);
    }

    // PROGRESS & ACCOUNTING TESTS (2 tests)

    [Fact]
    public async Task ProcessGenerationAsync_ThreeChunks_UpdatesProgressCorrectly()
    {
       // Arrange
       var generation = CreateGeneration();
       var voice = CreateVoice(generation.VoiceId);
       var ttsResult = CreateSuccessfulTtsResult();

       _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
           .ReturnsAsync(generation);
       _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(voice);

       var textChunks = new List<TextChunk>
       {
           new TextChunk { Index = 0, Text = "Chunk 1", StartPosition = 0, EndPosition = 7 },
           new TextChunk { Index = 1, Text = "Chunk 2", StartPosition = 7, EndPosition = 14 },
           new TextChunk { Index = 2, Text = "Chunk 3", StartPosition = 14, EndPosition = 21 }
       };
       _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
           .Returns(textChunks);

       var mockProvider = SetupMockProvider();
       mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(ttsResult);

       SetupChunkAccessorDefaults();

       var mergeResult = new AudioMergeResult
       {
           AudioData = new byte[] { 1, 2, 3 },
           ContentType = "audio/mpeg",
           DurationMs = 3000,
           SizeBytes = 15
       };
       SetupCompletionPipeline(mergeResult);

       var processor = CreateProcessor();

       // Act
       await processor.ProcessGenerationAsync(generation.Id);

       // Assert
       _mockGenerationAccessor.Verify(x => x.UpdateProgressAsync(generation.Id, 1, 33, It.IsAny<CancellationToken>()), Times.Once);
       _mockGenerationAccessor.Verify(x => x.UpdateProgressAsync(generation.Id, 2, 66, It.IsAny<CancellationToken>()), Times.Once);
       _mockGenerationAccessor.Verify(x => x.UpdateProgressAsync(generation.Id, 3, 100, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessGenerationAsync_ThreeChunks_AccumulatesCostAndDuration()
    {
        // Arrange
        var generation = CreateGeneration();
        var voice = CreateVoice(generation.VoiceId);

        _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);
        _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(voice);

        var textChunks = new List<TextChunk>
        {
            new TextChunk { Index = 0, Text = "Chunk 1", StartPosition = 0, EndPosition = 7 },
            new TextChunk { Index = 1, Text = "Chunk 2", StartPosition = 7, EndPosition = 14 },
            new TextChunk { Index = 2, Text = "Chunk 3", StartPosition = 14, EndPosition = 21 }
        };
        _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
            .Returns(textChunks);

        var mockProvider = SetupMockProvider();
        var callCount = 0;
        mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return new TtsResult
                {
                    Success = true,
                    AudioData = new byte[] { 1, 2, 3 },
                    ContentType = "audio/mpeg",
                    DurationMs = 1000 + (callCount * 100),
                    Cost = 0.01m + (callCount * 0.005m),
                    CharactersProcessed = 7
                };
            });

        SetupChunkAccessorDefaults();

        var mergeResult = new AudioMergeResult
        {
            AudioData = new byte[] { 1, 2, 3 },
            ContentType = "audio/mpeg",
            DurationMs = 3300,
            SizeBytes = 15
        };
        SetupCompletionPipeline(mergeResult);

        var processor = CreateProcessor();

        // Act
        await processor.ProcessGenerationAsync(generation.Id);

        // Assert
        var expectedTotalCost = 0.015m + 0.020m + 0.025m; // 0.06m
        _mockGenerationAccessor.Verify(x => x.SetCompletedAsync(
            generation.Id,
            It.IsAny<string>(),
            "mp3",
            3300,
            15,
            expectedTotalCost,
            It.IsAny<CancellationToken>()), Times.Once);
        _mockUserAccessor.Verify(x => x.TryDeductCreditsAsync(
            generation.UserId, 6, It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // CREDIT DEDUCTION RESILIENCE TESTS (5 tests — including test from Task 1)

    [Fact]
    public async Task ProcessGenerationAsync_CreditDeductionFails_GenerationStaysCompleted()
    {
        // Arrange — same as SingleChunk happy path
        var generation = CreateGeneration();
        var voice = CreateVoice(generation.VoiceId);
        var ttsResult = CreateSuccessfulTtsResult();

        _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);
        _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(voice);

        var textChunks = new List<TextChunk>
        {
            new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
        };
        _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
            .Returns(textChunks);

        var mockProvider = SetupMockProvider();
        mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ttsResult);

        SetupChunkAccessorDefaults();

        var mergeResult = new AudioMergeResult
        {
            AudioData = new byte[] { 1, 2, 3 },
            ContentType = "audio/mpeg",
            DurationMs = 1000,
            SizeBytes = 5
        };
        SetupCompletionPipeline(mergeResult);

        // OVERRIDE: Make credit deduction always fail
        _mockUserAccessor.Setup(x => x.TryDeductCreditsAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Credit service unavailable"));

        _mockDelayService.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var processor = CreateProcessor();

        // Act
        await processor.ProcessGenerationAsync(generation.Id);

        // Assert — generation stays Completed, never transitions to Failed
        _mockGenerationAccessor.Verify(x => x.SetCompletedAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<long>(), It.IsAny<decimal>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(
            generation.Id, GenerationStatus.Failed,
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessGenerationAsync_CreditDeductionFailsOnce_RetriesAndSucceeds()
    {
        // Arrange — same as SingleChunk happy path
        var generation = CreateGeneration();
        var voice = CreateVoice(generation.VoiceId);
        var ttsResult = CreateSuccessfulTtsResult();

        _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);
        _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(voice);

        var textChunks = new List<TextChunk>
        {
            new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
        };
        _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
            .Returns(textChunks);

        var mockProvider = SetupMockProvider();
        mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ttsResult);

        SetupChunkAccessorDefaults();

        var mergeResult = new AudioMergeResult
        {
            AudioData = new byte[] { 1, 2, 3 },
            ContentType = "audio/mpeg",
            DurationMs = 1000,
            SizeBytes = 5
        };
        SetupCompletionPipeline(mergeResult);

        // OVERRIDE: Make credit deduction fail once, then succeed
        var callCount = 0;
        _mockUserAccessor.Setup(x => x.TryDeductCreditsAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                    throw new InvalidOperationException("Credit service temporarily unavailable");
                return Task.FromResult(true);
            });

        _mockDelayService.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var processor = CreateProcessor();

        // Act
        await processor.ProcessGenerationAsync(generation.Id);

        // Assert
        _mockUserAccessor.Verify(x => x.TryDeductCreditsAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockDelayService.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(1), CancellationToken.None), Times.Once);
        _mockGenerationAccessor.Verify(x => x.SetCompletedAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<long>(), It.IsAny<decimal>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessGenerationAsync_CreditDeductionExhaustsRetries_GenerationStaysCompleted()
    {
        // Arrange — same as SingleChunk happy path
        var generation = CreateGeneration();
        var voice = CreateVoice(generation.VoiceId);
        var ttsResult = CreateSuccessfulTtsResult();

        _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);
        _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(voice);

        var textChunks = new List<TextChunk>
        {
            new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
        };
        _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
            .Returns(textChunks);

        var mockProvider = SetupMockProvider();
        mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ttsResult);

        SetupChunkAccessorDefaults();

        var mergeResult = new AudioMergeResult
        {
            AudioData = new byte[] { 1, 2, 3 },
            ContentType = "audio/mpeg",
            DurationMs = 1000,
            SizeBytes = 5
        };
        SetupCompletionPipeline(mergeResult);

        // OVERRIDE: Make credit deduction always fail
        _mockUserAccessor.Setup(x => x.TryDeductCreditsAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Credit service unavailable"));

        _mockDelayService.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var processor = CreateProcessor();

        // Act
        await processor.ProcessGenerationAsync(generation.Id);

        // Assert
        _mockUserAccessor.Verify(x => x.TryDeductCreditsAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
        _mockGenerationAccessor.Verify(x => x.SetCompletedAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<long>(), It.IsAny<decimal>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(
            generation.Id, GenerationStatus.Failed,
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("BILLING")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessGenerationAsync_CreditDeductionRetry_UsesExpectedBackoffDelays()
    {
        // Arrange — same as SingleChunk happy path
        var generation = CreateGeneration();
        var voice = CreateVoice(generation.VoiceId);
        var ttsResult = CreateSuccessfulTtsResult();

        _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);
        _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(voice);

        var textChunks = new List<TextChunk>
        {
            new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
        };
        _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
            .Returns(textChunks);

        var mockProvider = SetupMockProvider();
        mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ttsResult);

        SetupChunkAccessorDefaults();

        var mergeResult = new AudioMergeResult
        {
            AudioData = new byte[] { 1, 2, 3 },
            ContentType = "audio/mpeg",
            DurationMs = 1000,
            SizeBytes = 5
        };
        SetupCompletionPipeline(mergeResult);

        // OVERRIDE: Make credit deduction fail twice, succeed on third
        var callCount = 0;
        _mockUserAccessor.Setup(x => x.TryDeductCreditsAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount <= 2)
                    throw new InvalidOperationException("Credit service temporarily unavailable");
                return Task.FromResult(true);
            });

        _mockDelayService.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var processor = CreateProcessor();

        // Act
        await processor.ProcessGenerationAsync(generation.Id);

        // Assert
        _mockDelayService.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(1), CancellationToken.None), Times.Once);
        _mockDelayService.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(3), CancellationToken.None), Times.Once);
        _mockDelayService.Verify(x => x.DelayAsync(TimeSpan.FromSeconds(10), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task ProcessGenerationAsync_CreditDeductionRetry_UsesCancellationTokenNone()
    {
        // Arrange — same as SingleChunk happy path
        var generation = CreateGeneration();
        var voice = CreateVoice(generation.VoiceId);
        var ttsResult = CreateSuccessfulTtsResult();

        _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);
        _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(voice);

        var textChunks = new List<TextChunk>
        {
            new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
        };
        _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
            .Returns(textChunks);

        var mockProvider = SetupMockProvider();
        mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ttsResult);

        SetupChunkAccessorDefaults();

        var mergeResult = new AudioMergeResult
        {
            AudioData = new byte[] { 1, 2, 3 },
            ContentType = "audio/mpeg",
            DurationMs = 1000,
            SizeBytes = 5
        };
        SetupCompletionPipeline(mergeResult);

        // OVERRIDE: Make credit deduction fail once, then succeed
        var callCount = 0;
        _mockUserAccessor.Setup(x => x.TryDeductCreditsAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                    throw new InvalidOperationException("Credit service temporarily unavailable");
                return Task.FromResult(true);
            });

         _mockDelayService.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

         var processor = CreateProcessor();

         // Act
         await processor.ProcessGenerationAsync(generation.Id);

         // Assert — ALL TryDeductCreditsAsync calls received CancellationToken.None
          _mockUserAccessor.Verify(x => x.TryDeductCreditsAsync(
              It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), CancellationToken.None), Times.Exactly(2));
     }

     [Fact]
     public async Task ProcessGenerationAsync_PendingGeneration_SetsStatusToProcessing_WhichPersistsStartedAt()
     {
         // Arrange
         var generation = CreateGeneration();
         var voice = CreateVoice(generation.VoiceId);
         var ttsResult = CreateSuccessfulTtsResult();

         _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(generation);
         _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(voice);

         _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
             .Returns(new List<TextChunk>
             {
                 new TextChunk { Index = 0, Text = "Test text for generation", StartPosition = 0, EndPosition = 24 }
             });

         var mockProvider = SetupMockProvider();
         mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(ttsResult);

         SetupChunkAccessorDefaults();

         var mergeResult = new AudioMergeResult
         {
             AudioData = new byte[] { 1, 2, 3, 4, 5 },
             ContentType = "audio/mpeg",
             DurationMs = 1000,
             SizeBytes = 5
         };
         SetupCompletionPipeline(mergeResult);

         var processor = CreateProcessor();

         // Act
         await processor.ProcessGenerationAsync(generation.Id);

         // Assert
         // Verify that UpdateStatusAsync is called with GenerationStatus.Processing
         // This confirms that StartedAt is set at the DB level by the accessor
         _mockGenerationAccessor.Verify(x => x.UpdateStatusAsync(
             generation.Id,
             GenerationStatus.Processing,
             null,
             It.IsAny<CancellationToken>()), Times.Once);
     }

     [Fact]
     public async Task ProcessGenerationAsync_CreditDeduction_PassesGenerationIdAsIdempotencyKey()
     {
         // Arrange — same as SingleChunk happy path
         var generation = CreateGeneration();
         var voice = CreateVoice(generation.VoiceId);
         var ttsResult = CreateSuccessfulTtsResult();

         _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(generation);
         _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(voice);

         var textChunks = new List<TextChunk>
         {
             new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
         };
         _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
             .Returns(textChunks);

         var mockProvider = SetupMockProvider();
         mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(ttsResult);

         SetupChunkAccessorDefaults();

         var mergeResult = new AudioMergeResult
         {
             AudioData = new byte[] { 1, 2, 3 },
             ContentType = "audio/mpeg",
             DurationMs = 1000,
             SizeBytes = 5
         };
         SetupCompletionPipeline(mergeResult);

         _mockUserAccessor.Setup(x => x.TryDeductCreditsAsync(
             It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

         var processor = CreateProcessor();

         // Act
         await processor.ProcessGenerationAsync(generation.Id);

         // Assert — verify generationId is passed for both idempotencyKey and generationId params
         _mockUserAccessor.Verify(x => x.TryDeductCreditsAsync(
             generation.UserId,
             It.IsAny<int>(),
             generation.Id,  // idempotencyKey
             generation.Id,  // generationId
             CancellationToken.None), Times.Once);
     }

     [Fact]
     public async Task ProcessGenerationAsync_CreditDeductionDuplicate_LogsWarningAndCompletes()
     {
         // Arrange — same as SingleChunk happy path
         var generation = CreateGeneration();
         var voice = CreateVoice(generation.VoiceId);
         var ttsResult = CreateSuccessfulTtsResult();

         _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(generation);
         _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(voice);

         var textChunks = new List<TextChunk>
         {
             new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
         };
         _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
             .Returns(textChunks);

         var mockProvider = SetupMockProvider();
         mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(ttsResult);

         SetupChunkAccessorDefaults();

         var mergeResult = new AudioMergeResult
         {
             AudioData = new byte[] { 1, 2, 3 },
             ContentType = "audio/mpeg",
             DurationMs = 1000,
             SizeBytes = 5
         };
         SetupCompletionPipeline(mergeResult);

         // OVERRIDE: Make credit deduction return false (duplicate)
         _mockUserAccessor.Setup(x => x.TryDeductCreditsAsync(
             It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);

         var processor = CreateProcessor();

         // Act
         await processor.ProcessGenerationAsync(generation.Id);

         // Assert — generation stays Completed
         _mockGenerationAccessor.Verify(x => x.SetCompletedAsync(
             It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
             It.IsAny<int>(), It.IsAny<long>(), It.IsAny<decimal>(),
             It.IsAny<CancellationToken>()), Times.Once);

         // Assert — LogWarning emitted for duplicate
         _mockLogger.Verify(
             x => x.Log(
                 LogLevel.Warning,
                 It.IsAny<EventId>(),
                 It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("already applied")),
                 It.IsAny<Exception?>(),
                 It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
             Times.Once);
     }

     [Fact]
     public async Task ProcessGenerationAsync_CreditDeductionDuplicate_DoesNotRetry()
     {
         // Arrange — same as SingleChunk happy path
         var generation = CreateGeneration();
         var voice = CreateVoice(generation.VoiceId);
         var ttsResult = CreateSuccessfulTtsResult();

         _mockGenerationAccessor.Setup(x => x.GetByIdAsync(generation.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(generation);
         _mockVoiceAccessor.Setup(x => x.GetByIdAsync(generation.VoiceId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(voice);

         var textChunks = new List<TextChunk>
         {
             new TextChunk { Index = 0, Text = generation.InputText, StartPosition = 0, EndPosition = generation.InputText.Length }
         };
         _mockChunkingEngine.Setup(x => x.SplitText(generation.InputText, It.IsAny<ChunkingOptions?>()))
             .Returns(textChunks);

         var mockProvider = SetupMockProvider();
         mockProvider.Setup(x => x.GenerateSpeechAsync(It.IsAny<TtsRequest>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(ttsResult);

         SetupChunkAccessorDefaults();

         var mergeResult = new AudioMergeResult
         {
             AudioData = new byte[] { 1, 2, 3 },
             ContentType = "audio/mpeg",
             DurationMs = 1000,
             SizeBytes = 5
         };
         SetupCompletionPipeline(mergeResult);

         // OVERRIDE: Make credit deduction return false (duplicate)
         _mockUserAccessor.Setup(x => x.TryDeductCreditsAsync(
             It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);

         var processor = CreateProcessor();

         // Act
         await processor.ProcessGenerationAsync(generation.Id);

         // Assert — TryDeductCreditsAsync called exactly once (no retries)
         _mockUserAccessor.Verify(x => x.TryDeductCreditsAsync(
             It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);

         // Assert — DelayAsync never called (no retry delay)
         _mockDelayService.Verify(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
     }
}
