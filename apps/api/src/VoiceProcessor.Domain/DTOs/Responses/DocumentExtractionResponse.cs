namespace VoiceProcessor.Domain.DTOs.Responses;

/// <summary>
/// Response DTO for document text extraction.
/// </summary>
public record DocumentExtractionResponse(
    string Text,
    int? PageCount,
    int WordCount,
    int CharacterCount);
