using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using VoiceProcessor.Clients.Api.Hubs;
using VoiceProcessor.Clients.Api.Notifications;
using VoiceProcessor.Domain.Contracts.Hubs;
using VoiceProcessor.Domain.DTOs.Notifications;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Clients.Api.Tests.Notifications;

public class GenerationNotificationAccessorTests
{
    private readonly Mock<IHubContext<GenerationHub, IGenerationClient>> _mockHubContext;
    private readonly Mock<IHubClients<IGenerationClient>> _mockClients;
    private readonly Mock<IGenerationClient> _mockClientProxy;
    private readonly Mock<ILogger<GenerationNotificationAccessor>> _mockLogger;
    private readonly GenerationNotificationAccessor _accessor;

    public GenerationNotificationAccessorTests()
    {
        _mockHubContext = new Mock<IHubContext<GenerationHub, IGenerationClient>>();
        _mockClients = new Mock<IHubClients<IGenerationClient>>();
        _mockClientProxy = new Mock<IGenerationClient>();
        _mockLogger = new Mock<ILogger<GenerationNotificationAccessor>>();

        _mockHubContext.Setup(x => x.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(x => x.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);

        _accessor = new GenerationNotificationAccessor(_mockHubContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SendStatusUpdateAsync_SendsToCorrectUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid();
        var status = GenerationStatus.Processing;

        // Act
        await _accessor.SendStatusUpdateAsync(userId, generationId, status);

        // Assert
        _mockClients.Verify(x => x.User(userId.ToString()), Times.Once);
    }

    [Fact]
    public async Task SendStatusUpdateAsync_MapsStatusCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid();
        var status = GenerationStatus.Processing;
        StatusUpdateNotification? capturedNotification = null;

        _mockClientProxy
            .Setup(x => x.StatusUpdate(It.IsAny<StatusUpdateNotification>()))
            .Callback<StatusUpdateNotification>(n => capturedNotification = n)
            .Returns(Task.CompletedTask);

        // Act
        await _accessor.SendStatusUpdateAsync(userId, generationId, status, "Test message");

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.GenerationId.Should().Be(generationId);
        capturedNotification.Status.Should().Be("processing");
        capturedNotification.Message.Should().Be("Test message");
    }

    [Fact]
    public async Task SendProgressAsync_SendsCorrectData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid();
        var progress = 50;
        var currentChunk = 5;
        var totalChunks = 10;
        ProgressNotification? capturedNotification = null;

        _mockClientProxy
            .Setup(x => x.Progress(It.IsAny<ProgressNotification>()))
            .Callback<ProgressNotification>(n => capturedNotification = n)
            .Returns(Task.CompletedTask);

        // Act
        await _accessor.SendProgressAsync(userId, generationId, progress, currentChunk, totalChunks);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.GenerationId.Should().Be(generationId);
        capturedNotification.Progress.Should().Be(progress);
        capturedNotification.CurrentChunk.Should().Be(currentChunk);
        capturedNotification.TotalChunks.Should().Be(totalChunks);
    }

    [Fact]
    public async Task SendCompletedAsync_SendsCorrectData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid();
        var audioUrl = "https://example.com/audio.mp3";
        var durationMs = 120000;
        CompletedNotification? capturedNotification = null;

        _mockClientProxy
            .Setup(x => x.Completed(It.IsAny<CompletedNotification>()))
            .Callback<CompletedNotification>(n => capturedNotification = n)
            .Returns(Task.CompletedTask);

        // Act
        await _accessor.SendCompletedAsync(userId, generationId, audioUrl, durationMs);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.GenerationId.Should().Be(generationId);
        capturedNotification.AudioUrl.Should().Be(audioUrl);
        capturedNotification.Duration.Should().Be(durationMs);
    }

    [Fact]
    public async Task SendFailedAsync_SendsCorrectData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid();
        var error = "Test error message";
        FailedNotification? capturedNotification = null;

        _mockClientProxy
            .Setup(x => x.Failed(It.IsAny<FailedNotification>()))
            .Callback<FailedNotification>(n => capturedNotification = n)
            .Returns(Task.CompletedTask);

        // Act
        await _accessor.SendFailedAsync(userId, generationId, error);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.GenerationId.Should().Be(generationId);
        capturedNotification.Error.Should().Be(error);
    }

    [Fact]
    public async Task SendStatusUpdateAsync_SwallowsException_WhenHubThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid();
        var status = GenerationStatus.Processing;

        _mockClientProxy
            .Setup(x => x.StatusUpdate(It.IsAny<StatusUpdateNotification>()))
            .ThrowsAsync(new Exception("Hub connection failed"));

        // Act
        var act = async () => await _accessor.SendStatusUpdateAsync(userId, generationId, status);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendProgressAsync_SwallowsException_WhenHubThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid();

        _mockClientProxy
            .Setup(x => x.Progress(It.IsAny<ProgressNotification>()))
            .ThrowsAsync(new Exception("Hub connection failed"));

        // Act
        var act = async () => await _accessor.SendProgressAsync(userId, generationId, 50);

        // Assert
        await act.Should().NotThrowAsync();
    }

     [Fact]
     public async Task SendCompletedAsync_SwallowsException_WhenHubThrows()
     {
         // Arrange
         var userId = Guid.NewGuid();
         var generationId = Guid.NewGuid();

         _mockClientProxy
             .Setup(x => x.Completed(It.IsAny<CompletedNotification>()))
             .ThrowsAsync(new Exception("Hub connection failed"));

         // Act
         var act = async () => await _accessor.SendCompletedAsync(userId, generationId, "url", 1000);

         // Assert
         await act.Should().NotThrowAsync();
     }

     [Fact]
     public async Task SendStatusUpdateAsync_LogsArgumentOutOfRangeException_ForUnknownStatus()
     {
         // Arrange
         var userId = Guid.NewGuid();
         var generationId = Guid.NewGuid();
         var unknownStatus = (GenerationStatus)999;

         _mockClientProxy
             .Setup(x => x.StatusUpdate(It.IsAny<StatusUpdateNotification>()))
             .Returns(Task.CompletedTask);

         // Act
         await _accessor.SendStatusUpdateAsync(userId, generationId, unknownStatus);

         // Assert
         _mockLogger.Verify(
             x => x.Log(
                 LogLevel.Warning,
                 It.IsAny<EventId>(),
                 It.IsAny<It.IsAnyType>(),
                 It.IsAny<ArgumentOutOfRangeException>(),
                 It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
             Times.Once);
     }

     [Theory]
     [InlineData(GenerationStatus.Pending, "queued")]
     [InlineData(GenerationStatus.Analyzing, "processing")]
     [InlineData(GenerationStatus.Chunking, "processing")]
     [InlineData(GenerationStatus.Processing, "processing")]
     [InlineData(GenerationStatus.Merging, "processing")]
     [InlineData(GenerationStatus.Completed, "completed")]
     [InlineData(GenerationStatus.Failed, "failed")]
     [InlineData(GenerationStatus.Cancelled, "cancelled")]
     public async Task SendStatusUpdateAsync_MapsAllEnumValues_Correctly(GenerationStatus status, string expectedMappedStatus)
     {
         // Arrange
         var userId = Guid.NewGuid();
         var generationId = Guid.NewGuid();
         StatusUpdateNotification? capturedNotification = null;

         _mockClientProxy
             .Setup(x => x.StatusUpdate(It.IsAny<StatusUpdateNotification>()))
             .Callback<StatusUpdateNotification>(n => capturedNotification = n)
             .Returns(Task.CompletedTask);

         // Act
         await _accessor.SendStatusUpdateAsync(userId, generationId, status);

         // Assert
         capturedNotification.Should().NotBeNull();
         capturedNotification!.Status.Should().Be(expectedMappedStatus);
     }

     [Fact]
     public async Task SendStatusUpdateAsync_DoesNotCallHub_WhenTokenCancelled()
     {
         // Arrange
         var userId = Guid.NewGuid();
         var generationId = Guid.NewGuid();
         var status = GenerationStatus.Processing;
         var cts = new CancellationTokenSource();
         cts.Cancel();

         // Act
         await _accessor.SendStatusUpdateAsync(userId, generationId, status, cancellationToken: cts.Token);

         // Assert
         _mockClientProxy.Verify(x => x.StatusUpdate(It.IsAny<StatusUpdateNotification>()), Times.Never);
     }

     [Fact]
     public async Task SendProgressAsync_DoesNotCallHub_WhenTokenCancelled()
     {
         // Arrange
         var userId = Guid.NewGuid();
         var generationId = Guid.NewGuid();
         var cts = new CancellationTokenSource();
         cts.Cancel();

         // Act
         await _accessor.SendProgressAsync(userId, generationId, 50, cancellationToken: cts.Token);

         // Assert
         _mockClientProxy.Verify(x => x.Progress(It.IsAny<ProgressNotification>()), Times.Never);
     }

     [Fact]
     public async Task SendCompletedAsync_DoesNotCallHub_WhenTokenCancelled()
     {
         // Arrange
         var userId = Guid.NewGuid();
         var generationId = Guid.NewGuid();
         var cts = new CancellationTokenSource();
         cts.Cancel();

         // Act
         await _accessor.SendCompletedAsync(userId, generationId, "url", 1000, cancellationToken: cts.Token);

         // Assert
         _mockClientProxy.Verify(x => x.Completed(It.IsAny<CompletedNotification>()), Times.Never);
     }

     [Fact]
     public async Task SendFailedAsync_DoesNotCallHub_WhenTokenCancelled()
     {
         // Arrange
         var userId = Guid.NewGuid();
         var generationId = Guid.NewGuid();
         var cts = new CancellationTokenSource();
         cts.Cancel();

         // Act
         await _accessor.SendFailedAsync(userId, generationId, "error", cancellationToken: cts.Token);

         // Assert
         _mockClientProxy.Verify(x => x.Failed(It.IsAny<FailedNotification>()), Times.Never);
     }
 }
