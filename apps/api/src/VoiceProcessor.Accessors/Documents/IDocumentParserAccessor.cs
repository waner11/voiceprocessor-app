namespace VoiceProcessor.Accessors.Documents;

public interface IDocumentParserAccessor
{
    Task<DocumentExtractionResult> ExtractTextAsync(Stream fileStream, string mimeType, string fileName);
}

public record DocumentExtractionResult(string Text, int? PageCount, int WordCount, int CharacterCount);
