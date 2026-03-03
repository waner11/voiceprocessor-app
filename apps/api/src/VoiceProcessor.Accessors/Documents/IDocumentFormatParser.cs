namespace VoiceProcessor.Accessors.Documents;

public interface IDocumentFormatParser
{
    IReadOnlyCollection<string> SupportedMimeTypes { get; }

    Task<DocumentExtractionResult> ExtractTextAsync(Stream fileStream, string mimeType, string fileName);
}
