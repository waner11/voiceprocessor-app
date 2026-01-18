using System.Text.Json;
using System.Text.Json.Serialization;

namespace VoiceProcessor.Accessors.Providers;

public static class AccessorJsonOptions
{
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
