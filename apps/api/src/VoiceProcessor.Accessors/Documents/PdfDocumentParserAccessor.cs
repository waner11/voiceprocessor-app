using VoiceProcessor.Domain.Exceptions;
using UglyToad.PdfPig;
using VoiceProcessor.Domain.DTOs.Documents;
using VoiceProcessor.Utilities.Text;
namespace VoiceProcessor.Accessors.Documents;

public class PdfDocumentParserAccessor : IDocumentFormatParser
{
    public IReadOnlyCollection<string> SupportedMimeTypes { get; } = ["application/pdf"];

    public Task<DocumentExtractionResult> ExtractTextAsync(Stream fileStream, string mimeType, string fileName, CancellationToken cancellationToken = default)
    {
        if (!SupportedMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"PDF parser does not support mime type '{mimeType}'.");
        }

        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        using var document = PdfDocument.Open(fileStream);
        var pageTexts = document.GetPages()
            .Select(page => page.Text.Trim())
            .ToList();

        var text = string.Join("\n\n", pageTexts.Where(pageText => !string.IsNullOrWhiteSpace(pageText)));

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DocumentParsingException($"PDF '{fileName}' does not contain extractable text. Image-only and password-protected PDFs are not supported.");
        }

        return Task.FromResult(DocumentTextMetrics.BuildResult(text, document.NumberOfPages));
    }
}
