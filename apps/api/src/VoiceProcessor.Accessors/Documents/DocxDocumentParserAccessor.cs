using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using VoiceProcessor.Domain.DTOs.Documents;
using VoiceProcessor.Utilities.Text;
namespace VoiceProcessor.Accessors.Documents;

public class DocxDocumentParserAccessor : IDocumentFormatParser
{
    public IReadOnlyCollection<string> SupportedMimeTypes { get; } =
        ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"];

    public Task<DocumentExtractionResult> ExtractTextAsync(Stream fileStream, string mimeType, string fileName, CancellationToken cancellationToken = default)
    {
        if (!SupportedMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"DOCX parser does not support mime type '{mimeType}'.");
        }

        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        using var wordDocument = WordprocessingDocument.Open(fileStream, false);
        var body = wordDocument.MainDocumentPart?.Document?.Body;

        if (body is null)
        {
            return Task.FromResult(DocumentTextMetrics.BuildResult(string.Empty, null));
        }

        var paragraphs = body.Elements<Paragraph>()
            .Select(ExtractParagraphText)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToList();

        var text = string.Join("\n\n", paragraphs);
        return Task.FromResult(DocumentTextMetrics.BuildResult(text, null));
    }

    private static string ExtractParagraphText(Paragraph paragraph)
    {
        return string.Concat(
                paragraph.Descendants<Text>()
                    .Where(text => !IsTrackedChangeText(text))
                    .Select(text => text.Text))
            .Trim();
    }

    private static bool IsTrackedChangeText(Text text)
    {
        return text.Ancestors().Any(ancestor =>
            ancestor.LocalName is "ins" or "del" or "commentRangeStart" or "commentRangeEnd" or "commentReference");
    }
}
