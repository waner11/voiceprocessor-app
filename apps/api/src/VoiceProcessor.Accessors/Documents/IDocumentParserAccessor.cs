using VoiceProcessor.Domain.DTOs.Documents;

namespace VoiceProcessor.Accessors.Documents;

public interface IDocumentParserAccessor
{
    Task<DocumentExtractionResult> ExtractTextAsync(Stream fileStream, string mimeType, string fileName);
}
