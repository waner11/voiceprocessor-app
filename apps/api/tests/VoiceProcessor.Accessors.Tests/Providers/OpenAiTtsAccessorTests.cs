using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Providers;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Tests.Providers;

public class OpenAiTtsAccessorTests
{
    private readonly Mock<ILogger<OpenAiTtsAccessor>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly OpenAiTtsOptions _options;

    public OpenAiTtsAccessorTests()
    {
        _loggerMock = new Mock<ILogger<OpenAiTtsAccessor>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };
        _options = new OpenAiTtsOptions
        {
            ApiKey = "test-api-key",
            DefaultModel = "tts-1",
            CostPerThousandChars = 0.015m
        };
    }

    [Fact]
    public async Task GenerateSpeechAsync_WithPresetSpeed_UsesMappedSpeed()
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

        var accessor = new OpenAiTtsAccessor(_httpClient, Options.Create(_options), _loggerMock.Object);

        var request = new TtsRequest
        {
            Text = "Test text",
            ProviderVoiceId = "alloy",
            OutputFormat = "mp3",
            Speed = 1.05
        };

        var result = await accessor.GenerateSpeechAsync(request);

        result.Success.Should().BeTrue();
        result.AudioData.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateSpeechAsync_WithoutPresetSpeed_UsesDefaultSpeed()
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

        var accessor = new OpenAiTtsAccessor(_httpClient, Options.Create(_options), _loggerMock.Object);

        var request = new TtsRequest
        {
            Text = "Test text",
            ProviderVoiceId = "alloy",
            OutputFormat = "mp3"
        };

        var result = await accessor.GenerateSpeechAsync(request);

        result.Success.Should().BeTrue();
        result.AudioData.Should().NotBeNull();
    }
}
