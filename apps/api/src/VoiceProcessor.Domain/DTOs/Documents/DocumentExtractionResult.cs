namespace VoiceProcessor.Domain.DTOs.Documents;

public record DocumentExtractionResult(string Text, int? PageCount, int WordCount, int CharacterCount);
