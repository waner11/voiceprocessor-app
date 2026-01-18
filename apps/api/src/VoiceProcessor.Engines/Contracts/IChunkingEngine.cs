namespace VoiceProcessor.Engines.Contracts;

public interface IChunkingEngine
{
    IReadOnlyList<TextChunk> SplitText(string text, ChunkingOptions? options = null);

    int EstimateChunkCount(string text, ChunkingOptions? options = null);
}

public record TextChunk
{
    public required int Index { get; init; }
    public required string Text { get; init; }
    public required int StartPosition { get; init; }
    public required int EndPosition { get; init; }
    public int CharacterCount => Text.Length;
}

public record ChunkingOptions
{
    public int MaxChunkSize { get; init; } = 5000;
    public int MinChunkSize { get; init; } = 100;
    public bool PreserveWordBoundaries { get; init; } = true;
    public bool PreserveSentenceBoundaries { get; init; } = true;
    public bool PreserveParagraphBoundaries { get; init; } = true;
}
