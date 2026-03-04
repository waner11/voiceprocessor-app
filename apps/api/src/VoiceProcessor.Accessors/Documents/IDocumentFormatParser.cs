using VoiceProcessor.Domain.DTOs.Documents;

namespace VoiceProcessor.Accessors.Documents;

public interface IDocumentFormatParser
{
    IReadOnlyCollection<string> SupportedMimeTypes { get; }

    Task<DocumentExtractionResult> ExtractTextAsync(Stream fileStream, string mimeType, string fileName, CancellationToken cancellationToken = default);
}
