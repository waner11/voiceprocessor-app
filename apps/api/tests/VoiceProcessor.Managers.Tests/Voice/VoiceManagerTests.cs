using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Providers;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Managers.Voice;

namespace VoiceProcessor.Managers.Tests.Voice;

public class VoiceManagerTests
{
    private readonly Mock<IVoiceAccessor> _mockVoiceAccessor;
    private readonly Mock<ITtsProviderFactory> _mockProviderFactory;
    private readonly Mock<ILogger<VoiceManager>> _mockLogger;

    public VoiceManagerTests()
    {
        _mockVoiceAccessor = new Mock<IVoiceAccessor>();
        _mockProviderFactory = new Mock<ITtsProviderFactory>();
        _mockLogger = new Mock<ILogger<VoiceManager>>();
    }

    private VoiceManager CreateManager()
    {
        return new VoiceManager(
            _mockVoiceAccessor.Object,
            _mockProviderFactory.Object,
            _mockLogger.Object
        );
    }

    #region GetVoicesAsync Tests

    [Fact]
    public async Task GetVoicesAsync_ValidRequest_ReturnsPagedResponse()
    {
        // Arrange
        var manager = CreateManager();
        var voices = new List<Domain.Entities.Voice>
        {
            new Domain.Entities.Voice
            {
                Id = Guid.NewGuid(),
                Name = "Voice 1",
                Description = "Test voice 1",
                Provider = Provider.ElevenLabs,
                ProviderVoiceId = "voice_1",
                Language = "en-US",
                Gender = "Female",
                CostPerThousandChars = 0.30m,
                IsActive = true
            },
            new Domain.Entities.Voice
            {
                Id = Guid.NewGuid(),
                Name = "Voice 2",
                Description = "Test voice 2",
                Provider = Provider.OpenAI,
                ProviderVoiceId = "voice_2",
                Language = "en-GB",
                Gender = "Male",
                CostPerThousandChars = 0.015m,
                IsActive = true
            }
        };

        _mockVoiceAccessor.Setup(x => x.GetPagedAsync(
                1, 50, null, null, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((voices, 2));

        // Act
        var result = await manager.GetVoicesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(50);
        result.Items[0].Name.Should().Be("Voice 1");
        result.Items[0].Provider.Should().Be(Provider.ElevenLabs);
        result.Items[1].Name.Should().Be("Voice 2");
        result.Items[1].Provider.Should().Be(Provider.OpenAI);
    }

    [Fact]
    public async Task GetVoicesAsync_WithFilters_PassesFiltersToAccessor()
    {
        // Arrange
        var manager = CreateManager();
        var voices = new List<Domain.Entities.Voice>
        {
            new Domain.Entities.Voice
            {
                Id = Guid.NewGuid(),
                Name = "Filtered Voice",
                Provider = Provider.ElevenLabs,
                ProviderVoiceId = "voice_filtered",
                Language = "en-US",
                Gender = "Female",
                CostPerThousandChars = 0.30m,
                IsActive = true
            }
        };

        _mockVoiceAccessor.Setup(x => x.GetPagedAsync(
                2, 25, Provider.ElevenLabs, "en-US", "Female", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((voices, 1));

        // Act
        var result = await manager.GetVoicesAsync(
            page: 2,
            pageSize: 25,
            provider: Provider.ElevenLabs,
            language: "en-US",
            gender: "Female");

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(25);
        result.Items[0].Language.Should().Be("en-US");
        result.Items[0].Gender.Should().Be("Female");
    }

    [Fact]
    public async Task GetVoicesAsync_EmptyResult_ReturnsEmptyPagedResponse()
    {
        // Arrange
        var manager = CreateManager();
        var emptyVoices = new List<Domain.Entities.Voice>();

        _mockVoiceAccessor.Setup(x => x.GetPagedAsync(
                1, 50, null, null, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((emptyVoices, 0));

        // Act
        var result = await manager.GetVoicesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(50);
    }

    #endregion

    #region GetVoiceAsync Tests

    [Fact]
    public async Task GetVoiceAsync_ExistingVoice_ReturnsVoiceResponse()
    {
        // Arrange
        var manager = CreateManager();
        var voiceId = Guid.NewGuid();
        var voice = new Domain.Entities.Voice
        {
            Id = voiceId,
            Name = "Test Voice",
            Description = "A test voice",
            Provider = Provider.ElevenLabs,
            ProviderVoiceId = "voice_123",
            Language = "en-US",
            Accent = "American",
            Gender = "Female",
            AgeGroup = "Young Adult",
            UseCase = "Narration",
            PreviewUrl = "https://example.com/preview.mp3",
            CostPerThousandChars = 0.30m,
            IsActive = true
        };

        _mockVoiceAccessor.Setup(x => x.GetByIdAsync(voiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(voice);

        // Act
        var result = await manager.GetVoiceAsync(voiceId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(voiceId);
        result.Name.Should().Be("Test Voice");
        result.Description.Should().Be("A test voice");
        result.Provider.Should().Be(Provider.ElevenLabs);
        result.Language.Should().Be("en-US");
        result.Accent.Should().Be("American");
        result.Gender.Should().Be("Female");
        result.AgeGroup.Should().Be("Young Adult");
        result.UseCase.Should().Be("Narration");
        result.PreviewUrl.Should().Be("https://example.com/preview.mp3");
        result.CostPerThousandChars.Should().Be(0.30m);
    }

    [Fact]
    public async Task GetVoiceAsync_NonExistentVoice_ReturnsNull()
    {
        // Arrange
        var manager = CreateManager();
        var voiceId = Guid.NewGuid();

        _mockVoiceAccessor.Setup(x => x.GetByIdAsync(voiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Voice?)null);

        // Act
        var result = await manager.GetVoiceAsync(voiceId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetVoicesByProviderAsync Tests

    [Fact]
    public async Task GetVoicesByProviderAsync_MultipleProviders_GroupsCorrectly()
    {
        // Arrange
        var manager = CreateManager();
        var voices = new List<Domain.Entities.Voice>
        {
            new Domain.Entities.Voice
            {
                Id = Guid.NewGuid(),
                Name = "ElevenLabs Voice 1",
                Provider = Provider.ElevenLabs,
                ProviderVoiceId = "el_1",
                CostPerThousandChars = 0.30m,
                IsActive = true
            },
            new Domain.Entities.Voice
            {
                Id = Guid.NewGuid(),
                Name = "ElevenLabs Voice 2",
                Provider = Provider.ElevenLabs,
                ProviderVoiceId = "el_2",
                CostPerThousandChars = 0.30m,
                IsActive = true
            },
            new Domain.Entities.Voice
            {
                Id = Guid.NewGuid(),
                Name = "OpenAI Voice 1",
                Provider = Provider.OpenAI,
                ProviderVoiceId = "openai_1",
                CostPerThousandChars = 0.015m,
                IsActive = true
            },
            new Domain.Entities.Voice
            {
                Id = Guid.NewGuid(),
                Name = "Google Voice 1",
                Provider = Provider.GoogleCloud,
                ProviderVoiceId = "google_1",
                CostPerThousandChars = 0.016m,
                IsActive = true
            }
        };

        _mockVoiceAccessor.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(voices);

        // Act
        var result = await manager.GetVoicesByProviderAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().ContainKey(Provider.ElevenLabs);
        result.Should().ContainKey(Provider.OpenAI);
        result.Should().ContainKey(Provider.GoogleCloud);
        
        result[Provider.ElevenLabs].Should().HaveCount(2);
        result[Provider.ElevenLabs][0].Name.Should().Be("ElevenLabs Voice 1");
        result[Provider.ElevenLabs][1].Name.Should().Be("ElevenLabs Voice 2");
        
        result[Provider.OpenAI].Should().HaveCount(1);
        result[Provider.OpenAI][0].Name.Should().Be("OpenAI Voice 1");
        
        result[Provider.GoogleCloud].Should().HaveCount(1);
        result[Provider.GoogleCloud][0].Name.Should().Be("Google Voice 1");
    }

    [Fact]
    public async Task GetVoicesByProviderAsync_EmptyResult_ReturnsEmptyDictionary()
    {
        // Arrange
        var manager = CreateManager();
        var emptyVoices = new List<Domain.Entities.Voice>();

        _mockVoiceAccessor.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyVoices);

        // Act
        var result = await manager.GetVoicesByProviderAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region RefreshVoiceCatalogAsync Tests

    [Fact]
    public async Task RefreshVoiceCatalogAsync_SuccessfulRefresh_UpsertsAllVoices()
    {
        // Arrange
        var manager = CreateManager();
        var mockProvider1 = new Mock<ITtsProviderAccessor>();
        mockProvider1.Setup(x => x.Provider).Returns(Provider.ElevenLabs);
        mockProvider1.Setup(x => x.GetVoicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProviderVoice>
            {
                new ProviderVoice
                {
                    ProviderVoiceId = "el_voice_1",
                    Name = "ElevenLabs Voice 1",
                    Description = "Test voice",
                    Language = "en-US",
                    Gender = "Female"
                },
                new ProviderVoice
                {
                    ProviderVoiceId = "el_voice_2",
                    Name = "ElevenLabs Voice 2",
                    Description = "Another test voice",
                    Language = "en-GB",
                    Gender = "Male"
                }
            });

        var mockProvider2 = new Mock<ITtsProviderAccessor>();
        mockProvider2.Setup(x => x.Provider).Returns(Provider.OpenAI);
        mockProvider2.Setup(x => x.GetVoicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProviderVoice>
            {
                new ProviderVoice
                {
                    ProviderVoiceId = "openai_voice_1",
                    Name = "OpenAI Voice 1",
                    Language = "en-US"
                }
            });

        _mockProviderFactory.Setup(x => x.GetAllProviders())
            .Returns(new List<ITtsProviderAccessor> { mockProvider1.Object, mockProvider2.Object });

        // Act
        await manager.RefreshVoiceCatalogAsync();

        // Assert
        _mockVoiceAccessor.Verify(x => x.UpsertAsync(
            It.Is<Domain.Entities.Voice>(v => 
                v.Provider == Provider.ElevenLabs && 
                v.ProviderVoiceId == "el_voice_1" &&
                v.Name == "ElevenLabs Voice 1" &&
                v.CostPerThousandChars == 0.30m &&
                v.IsActive == true),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockVoiceAccessor.Verify(x => x.UpsertAsync(
            It.Is<Domain.Entities.Voice>(v => 
                v.Provider == Provider.ElevenLabs && 
                v.ProviderVoiceId == "el_voice_2"),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockVoiceAccessor.Verify(x => x.UpsertAsync(
            It.Is<Domain.Entities.Voice>(v => 
                v.Provider == Provider.OpenAI && 
                v.ProviderVoiceId == "openai_voice_1" &&
                v.CostPerThousandChars == 0.015m),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockVoiceAccessor.Verify(x => x.UpsertAsync(
            It.IsAny<Domain.Entities.Voice>(), 
            It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task RefreshVoiceCatalogAsync_ProviderFailure_ContinuesWithOtherProviders()
    {
        // Arrange
        var manager = CreateManager();
        var mockProvider1 = new Mock<ITtsProviderAccessor>();
        mockProvider1.Setup(x => x.Provider).Returns(Provider.ElevenLabs);
        mockProvider1.Setup(x => x.GetVoicesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Provider API error"));

        var mockProvider2 = new Mock<ITtsProviderAccessor>();
        mockProvider2.Setup(x => x.Provider).Returns(Provider.OpenAI);
        mockProvider2.Setup(x => x.GetVoicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProviderVoice>
            {
                new ProviderVoice
                {
                    ProviderVoiceId = "openai_voice_1",
                    Name = "OpenAI Voice 1",
                    Language = "en-US"
                }
            });

        _mockProviderFactory.Setup(x => x.GetAllProviders())
            .Returns(new List<ITtsProviderAccessor> { mockProvider1.Object, mockProvider2.Object });

        // Act
        await manager.RefreshVoiceCatalogAsync();

        // Assert
        _mockVoiceAccessor.Verify(x => x.UpsertAsync(
            It.Is<Domain.Entities.Voice>(v => v.Provider == Provider.OpenAI),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockVoiceAccessor.Verify(x => x.UpsertAsync(
            It.Is<Domain.Entities.Voice>(v => v.Provider == Provider.ElevenLabs),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshVoiceCatalogAsync_EmptyProviders_CompletesWithoutError()
    {
        // Arrange
        var manager = CreateManager();
        _mockProviderFactory.Setup(x => x.GetAllProviders())
            .Returns(new List<ITtsProviderAccessor>());

        // Act
        await manager.RefreshVoiceCatalogAsync();

        // Assert
        _mockVoiceAccessor.Verify(x => x.UpsertAsync(
            It.IsAny<Domain.Entities.Voice>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
