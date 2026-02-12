using System.Text.RegularExpressions;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Chunking;

public partial class ChunkingEngine : IChunkingEngine
{
    private static readonly ChunkingOptions DefaultOptions = new();

    public IReadOnlyList<TextChunk> SplitText(string text, ChunkingOptions? options = null)
    {
        if (string.IsNullOrEmpty(text))
            return new List<TextChunk>();

        options ??= DefaultOptions;

        if (text.Length <= options.MaxChunkSize)
        {
            return new List<TextChunk>
            {
                new TextChunk
                {
                    Index = 0,
                    Text = text,
                    StartPosition = 0,
                    EndPosition = text.Length
                }
            };
        }

        var chunks = new List<TextChunk>();
        var currentPosition = 0;
        var chunkIndex = 0;

        while (currentPosition < text.Length)
        {
            var remainingLength = text.Length - currentPosition;
            var chunkSize = Math.Min(options.MaxChunkSize, remainingLength);

            if (remainingLength <= options.MaxChunkSize)
            {
                // Last chunk - take everything remaining
                chunks.Add(new TextChunk
                {
                    Index = chunkIndex,
                    Text = text[currentPosition..],
                    StartPosition = currentPosition,
                    EndPosition = text.Length
                });
                break;
            }

            // Find the best break point
            var breakPoint = FindBreakPoint(text, currentPosition, chunkSize, options);
            var chunkText = text.Substring(currentPosition, breakPoint - currentPosition);

            chunks.Add(new TextChunk
            {
                Index = chunkIndex,
                Text = chunkText,
                StartPosition = currentPosition,
                EndPosition = breakPoint
            });

            currentPosition = breakPoint;
            chunkIndex++;

            // Skip leading whitespace for next chunk
            while (currentPosition < text.Length && char.IsWhiteSpace(text[currentPosition]))
            {
                currentPosition++;
            }
        }

        return chunks;
    }

    public int EstimateChunkCount(string text, ChunkingOptions? options = null)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        options ??= DefaultOptions;

        if (text.Length <= options.MaxChunkSize)
            return 1;

        // Rough estimate based on average chunk utilization (~85% of max)
        var avgChunkSize = (int)(options.MaxChunkSize * 0.85);
        return (int)Math.Ceiling((double)text.Length / avgChunkSize);
    }

    private static int FindBreakPoint(string text, int start, int maxLength, ChunkingOptions options)
    {
        var searchEnd = Math.Min(start + maxLength, text.Length);
        var searchStart = Math.Max(start + options.MinChunkSize, start);

        // Try to find paragraph break first
        if (options.PreserveParagraphBoundaries)
        {
            var paragraphBreak = FindLastParagraphBreak(text, searchStart, searchEnd);
            if (paragraphBreak > 0)
                return paragraphBreak;
        }

        // Try to find sentence break
        if (options.PreserveSentenceBoundaries)
        {
            var sentenceBreak = FindLastSentenceBreak(text, searchStart, searchEnd);
            if (sentenceBreak > 0)
                return sentenceBreak;
        }

        // Try to find word break
        if (options.PreserveWordBoundaries)
        {
            var wordBreak = FindLastWordBreak(text, searchStart, searchEnd);
            if (wordBreak > 0)
                return wordBreak;
        }

        // Fall back to max length
        return searchEnd;
    }

    private static int FindLastParagraphBreak(string text, int start, int end)
    {
        // Look for double newline (paragraph separator)
        for (var i = end - 1; i >= start; i--)
        {
            if (i > 0 && text[i] == '\n' && text[i - 1] == '\n')
                return i + 1;
            if (i > 1 && text[i] == '\n' && text[i - 1] == '\r' && text[i - 2] == '\n')
                return i + 1;
        }
        return -1;
    }

    private static int FindLastSentenceBreak(string text, int start, int end)
    {
        // Look for sentence-ending punctuation followed by space or newline
        for (var i = end - 1; i >= start; i--)
        {
            if (IsSentenceEnd(text, i))
                return i + 1;
        }
        return -1;
    }

    private static bool IsSentenceEnd(string text, int position)
    {
        if (position >= text.Length - 1)
            return false;

        var current = text[position];
        var next = text[position + 1];

        // Check for .!? followed by whitespace
        if ((current == '.' || current == '!' || current == '?') &&
            (char.IsWhiteSpace(next) || next == '"' || next == '\''))
        {
            // Avoid breaking on abbreviations like "Dr." "Mr." etc.
            if (current == '.' && position >= 2)
            {
                var prevChar = text[position - 1];
                if (char.IsUpper(prevChar) && (position < 2 || char.IsWhiteSpace(text[position - 2])))
                    return false; // Likely an abbreviation
            }
            return true;
        }

        return false;
    }

    private static int FindLastWordBreak(string text, int start, int end)
    {
        for (var i = end - 1; i >= start; i--)
        {
            if (char.IsWhiteSpace(text[i]))
                return i + 1;
        }
        return -1;
    }
}
