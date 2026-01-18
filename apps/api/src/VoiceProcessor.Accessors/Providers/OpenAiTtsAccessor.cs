using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Providers;

public class OpenAiTtsAccessor : ITtsProviderAccessor
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiTtsAccessor> _logger;
    private readonly OpenAiTtsOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // Available voices for tts-1 and tts-1-hd models
    private static readonly string[] AvailableVoices =
        ["alloy", "ash", "coral", "echo", "fable", "onyx", "nova", "sage", "shimmer"];

    public OpenAiTtsAccessor(
        HttpClient httpClient,
        IOptions<OpenAiTtsOptions> options,
        ILogger<OpenAiTtsAccessor> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Provider Provider => Provider.OpenAI;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("models", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenAI availability check failed");
            return false;
        }
    }

    public async Task<TtsResult> GenerateSpeechAsync(
        TtsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new OpenAiTtsRequest
            {
                Model = _options.DefaultModel,
                Input = request.Text,
                Voice = request.ProviderVoiceId,
                ResponseFormat = MapOutputFormat(request.OutputFormat),
                Speed = request.Speed ?? 1.0
            };

            var response = await _httpClient.PostAsJsonAsync(
                "audio/speech",
                payload,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenAI TTS failed: {StatusCode} - {Error}",
                    response.StatusCode, error);

                return new TtsResult
                {
                    Success = false,
                    ErrorMessage = $"OpenAI API error: {response.StatusCode}"
                };
            }

            var audioData = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var cost = CalculateCost(request.Text.Length);

            return new TtsResult
            {
                Success = true,
                AudioData = audioData,
                ContentType = GetContentType(request.OutputFormat),
                CharactersProcessed = request.Text.Length,
                Cost = cost
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI TTS generation failed");
            return new TtsResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public Task<IReadOnlyList<ProviderVoice>> GetVoicesAsync(
        CancellationToken cancellationToken = default)
    {
        // OpenAI has a fixed set of voices, not fetched from API
        var voices = AvailableVoices.Select(voice => new ProviderVoice
        {
            ProviderVoiceId = voice,
            Name = char.ToUpper(voice[0]) + voice[1..],
            Description = GetVoiceDescription(voice),
            Language = "en",
            Gender = GetVoiceGender(voice)
        }).ToList();

        return Task.FromResult<IReadOnlyList<ProviderVoice>>(voices);
    }

    private static string MapOutputFormat(string format) => format.ToLowerInvariant() switch
    {
        "mp3" => "mp3",
        "opus" => "opus",
        "aac" => "aac",
        "flac" => "flac",
        "wav" => "wav",
        "pcm" => "pcm",
        _ => "mp3"
    };

    private static string GetContentType(string format) => format.ToLowerInvariant() switch
    {
        "mp3" => "audio/mpeg",
        "opus" => "audio/opus",
        "aac" => "audio/aac",
        "flac" => "audio/flac",
        "wav" => "audio/wav",
        "pcm" => "audio/pcm",
        _ => "audio/mpeg"
    };

    private static string GetVoiceDescription(string voice) => voice switch
    {
        "alloy" => "Neutral and balanced voice",
        "ash" => "Warm and engaging voice",
        "coral" => "Clear and expressive voice",
        "echo" => "Soft and gentle voice",
        "fable" => "Expressive British-accented voice",
        "onyx" => "Deep and authoritative voice",
        "nova" => "Friendly and warm voice",
        "sage" => "Wise and measured voice",
        "shimmer" => "Bright and optimistic voice",
        _ => "AI-generated voice"
    };

    private static string GetVoiceGender(string voice) => voice switch
    {
        "alloy" => "Neutral",
        "echo" => "Male",
        "fable" => "Male",
        "onyx" => "Male",
        "nova" => "Female",
        "shimmer" => "Female",
        _ => "Neutral"
    };

    private decimal CalculateCost(int characterCount)
    {
        // OpenAI TTS pricing: $15.00 per 1M characters for tts-1, $30.00 for tts-1-hd
        return characterCount * _options.CostPerThousandChars / 1000m;
    }
}

public class OpenAiTtsOptions
{
    public const string SectionName = "OpenAiTts";

    public required string ApiKey { get; set; }
    public string DefaultModel { get; set; } = "tts-1";
    public decimal CostPerThousandChars { get; set; } = 0.015m; // $15/1M = $0.015/1K
}

internal class OpenAiTtsRequest
{
    public required string Model { get; set; }
    public required string Input { get; set; }
    public required string Voice { get; set; }
    public string ResponseFormat { get; set; } = "mp3";
    public double Speed { get; set; } = 1.0;
}
