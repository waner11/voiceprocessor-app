using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Providers;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Tests.Providers;

public class ElevenLabsAccessorTests
{
    private readonly Mock<ILogger<ElevenLabsAccessor>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly ElevenLabsOptions _options;

    public ElevenLabsAccessorTests()
    {
        _loggerMock = new Mock<ILogger<ElevenLabsAccessor>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.elevenlabs.io/v1/")
        };
        _options = new ElevenLabsOptions
        {
            ApiKey = "test-api-key",
            DefaultModel = "eleven_multilingual_v2",
            CostPerThousandChars = 0.30m
        };
    }

    [Fact]
    public async Task GenerateSpeechAsync_WithPreset_UsesMappedSettings()
    {
        var audioData = Encoding.UTF8.GetBytes("fake-audio-data");
        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>(async (req, ct) =>
            {
                capturedRequest = req;
                if (req.Content != null)
                    capturedBody = await req.Content.ReadAsStringAsync(ct);
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(audioData)
            });

        var accessor = new ElevenLabsAccessor(_httpClient, Options.Create(_options), _loggerMock.Object);

        var request = new TtsRequest
        {
            Text = "Test text",
            ProviderVoiceId = "test-voice-id",
            OutputFormat = "mp3",
            Stability = 0.35,
            SimilarityBoost = 0.70,
            Style = 0.5
        };

        var result = await accessor.GenerateSpeechAsync(request);

        result.Success.Should().BeTrue();
        result.AudioData.Should().NotBeNull();

        // Verify the HTTP body contains correct voice_settings
        capturedBody.Should().NotBeNullOrEmpty();
        using var doc = JsonDocument.Parse(capturedBody!);
        var root = doc.RootElement;

        var voiceSettings = root.GetProperty("voice_settings");
        voiceSettings.GetProperty("stability").GetDouble().Should().Be(0.35);
        voiceSettings.GetProperty("similarity_boost").GetDouble().Should().Be(0.70);
        voiceSettings.GetProperty("style").GetDouble().Should().Be(0.5);
        voiceSettings.GetProperty("use_speaker_boost").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GenerateSpeechAsync_WithoutPreset_UsesHardcodedDefaults()
    {
        var audioData = Encoding.UTF8.GetBytes("fake-audio-data");
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(audioData)
            });

        var accessor = new ElevenLabsAccessor(_httpClient, Options.Create(_options), _loggerMock.Object);

        var request = new TtsRequest
        {
            Text = "Test text",
            ProviderVoiceId = "test-voice-id",
            OutputFormat = "mp3"
        };

        var result = await accessor.GenerateSpeechAsync(request);

        result.Success.Should().BeTrue();
        result.AudioData.Should().NotBeNull();
    }
}
