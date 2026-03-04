using System.Text.RegularExpressions;
using VoiceProcessor.Domain.DTOs.Documents;

namespace VoiceProcessor.Utilities.Text;

public static partial class DocumentTextMetrics
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    public static DocumentExtractionResult BuildResult(string text, int? pageCount)
    {
        var normalizedText = text.Trim();
        var wordCount = CountWords(normalizedText);

        return new DocumentExtractionResult(
            normalizedText,
            pageCount,
            wordCount,
            normalizedText.Length);
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var normalized = WhitespaceRegex().Replace(text.Trim(), " ");
        return normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
