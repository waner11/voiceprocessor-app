using VoiceProcessor.Domain.DTOs.Documents;
namespace VoiceProcessor.Accessors.Documents;

public class DocumentParserAccessor : IDocumentParserAccessor
{
    private readonly IReadOnlyCollection<IDocumentFormatParser> _parsers;

    public DocumentParserAccessor(IEnumerable<IDocumentFormatParser> parsers)
    {
        _parsers = parsers.ToList();
    }

    public Task<DocumentExtractionResult> ExtractTextAsync(Stream fileStream, string mimeType, string fileName, CancellationToken cancellationToken = default)
    {
        var parser = _parsers.FirstOrDefault(p =>
            p.SupportedMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase));

        if (parser is null)
        {
            throw new NotSupportedException(
                $"No document parser is configured for mime type '{mimeType}' ({fileName}).");
        }

        return parser.ExtractTextAsync(fileStream, mimeType, fileName, cancellationToken);
    }
}
