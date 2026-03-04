using VoiceProcessor.Domain.DTOs.Documents;

namespace VoiceProcessor.Managers.Contracts;

public interface IDocumentManager
{
    Task<DocumentExtractionResult> ExtractTextAsync(
        Stream fileStream,
        string mimeType,
        string fileName,
        CancellationToken cancellationToken = default);
}
