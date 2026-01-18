using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Accessors.Providers;

public class ElevenLabsAccessor : ITtsProviderAccessor
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ElevenLabsAccessor> _logger;
    private readonly ElevenLabsOptions _options;

    public ElevenLabsAccessor(
        HttpClient httpClient,
        IOptions<ElevenLabsOptions> options,
        ILogger<ElevenLabsAccessor> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Provider Provider => Provider.ElevenLabs;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("voices", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ElevenLabs availability check failed");
            return false;
        }
    }

    public async Task<TtsResult> GenerateSpeechAsync(
        TtsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new ElevenLabsTtsRequest
            {
                Text = request.Text,
                ModelId = _options.DefaultModel,
                VoiceSettings = new VoiceSettings
                {
                    Stability = 0.5,
                    SimilarityBoost = 0.75,
                    Style = 0.0,
                    UseSpeakerBoost = true
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"text-to-speech/{request.ProviderVoiceId}",
                payload,
                AccessorJsonOptions.Default,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("ElevenLabs TTS failed: {StatusCode} - {Error}",
                    response.StatusCode, error);

                return new TtsResult
                {
                    Success = false,
                    ErrorMessage = $"ElevenLabs API error: {response.StatusCode}"
                };
            }

            var audioData = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var cost = CalculateCost(request.Text.Length);

            return new TtsResult
            {
                Success = true,
                AudioData = audioData,
                ContentType = "audio/mpeg",
                CharactersProcessed = request.Text.Length,
                Cost = cost
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ElevenLabs TTS generation failed");
            return new TtsResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<IReadOnlyList<ProviderVoice>> GetVoicesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("voices", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ElevenLabsVoicesResponse>(
                AccessorJsonOptions.Default, cancellationToken);

            return result?.Voices.Select(v => new ProviderVoice
            {
                ProviderVoiceId = v.VoiceId,
                Name = v.Name,
                Description = v.Description,
                Language = v.Labels?.GetValueOrDefault("language"),
                Accent = v.Labels?.GetValueOrDefault("accent"),
                Gender = v.Labels?.GetValueOrDefault("gender"),
                AgeGroup = v.Labels?.GetValueOrDefault("age"),
                UseCase = v.Labels?.GetValueOrDefault("use_case"),
                PreviewUrl = v.PreviewUrl
            }).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch ElevenLabs voices");
            return [];
        }
    }

    private decimal CalculateCost(int characterCount)
    {
        // ElevenLabs pricing: approximately $0.30 per 1000 characters (varies by plan)
        return characterCount * _options.CostPerThousandChars / 1000m;
    }
}

public class ElevenLabsOptions
{
    public const string SectionName = "ElevenLabs";

    public required string ApiKey { get; set; }
    public string DefaultModel { get; set; } = "eleven_multilingual_v2";
    public decimal CostPerThousandChars { get; set; } = 0.30m;
}

internal class ElevenLabsTtsRequest
{
    public required string Text { get; set; }
    public string? ModelId { get; set; }
    public VoiceSettings? VoiceSettings { get; set; }
}

internal class VoiceSettings
{
    public double Stability { get; set; }
    public double SimilarityBoost { get; set; }
    public double Style { get; set; }
    public bool UseSpeakerBoost { get; set; }
}

internal class ElevenLabsVoicesResponse
{
    public List<ElevenLabsVoice> Voices { get; set; } = [];
}

internal class ElevenLabsVoice
{
    public required string VoiceId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? PreviewUrl { get; set; }
    public Dictionary<string, string>? Labels { get; set; }
}
