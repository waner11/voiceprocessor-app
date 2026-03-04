using VoiceProcessor.Domain.DTOs.Documents;

using System.Net;

namespace VoiceProcessor.Accessors.Documents;

public class DocumentParserAccessor : IDocumentParserAccessor
{
    private const long MaxFileSizeBytes = 50L * 1024 * 1024;

    private readonly IReadOnlyCollection<IDocumentFormatParser> _parsers;

    public DocumentParserAccessor(IEnumerable<IDocumentFormatParser> parsers)
    {
        _parsers = parsers.ToList();
    }

    public Task<DocumentExtractionResult> ExtractTextAsync(Stream fileStream, string mimeType, string fileName)
    {
        ThrowIfFileTooLarge(fileStream);

        var parser = _parsers.FirstOrDefault(p =>
            p.SupportedMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase));

        if (parser is null)
        {
            throw new NotSupportedException(
                $"No document parser is configured for mime type '{mimeType}' ({fileName}).");
        }

        return parser.ExtractTextAsync(fileStream, mimeType, fileName);
    }

    private static void ThrowIfFileTooLarge(Stream fileStream)
    {
        if (fileStream.CanSeek && fileStream.Length > MaxFileSizeBytes)
        {
            throw new DocumentParsingException("File exceeds the 50MB limit.")
            {
                StatusCode = HttpStatusCode.RequestEntityTooLarge
            };
        }
    }
}
