using System.Text.RegularExpressions;
using VoiceProcessor.Domain.DTOs;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Analysis;

public partial class ChapterDetectionEngine : IChapterDetectionEngine
{
    // Regex patterns for chapter detection (case-insensitive, start-of-line)
    [GeneratedRegex(@"^Chapter\s+(\d+)(?:\s*[:\-]\s*(.+))?$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ChapterNumberedRegex();

    [GeneratedRegex(@"^Ch\.?\s+(\d+)(?:\s*[:\-]\s*(.+))?$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ChapterAbbreviationRegex();

    [GeneratedRegex(@"^Part\s+(\d+)(?:\s*[:\-]\s*(.+))?$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex PartNumberedRegex();

    [GeneratedRegex(@"^Section\s+(\d+)(?:\s*[:\-]\s*(.+))?$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex SectionNumberedRegex();

    [GeneratedRegex(@"^Chapter\s+(Seventy-Eight|Seventy-Seven|Seventy-Three|Eighty-Eight|Eighty-Seven|Eighty-Three|Ninety-Eight|Ninety-Seven|Ninety-Three|Seventy-Five|Seventy-Four|Seventy-Nine|Thirty-Eight|Thirty-Seven|Thirty-Three|Twenty-Eight|Twenty-Seven|Twenty-Three|Eighty-Five|Eighty-Four|Eighty-Nine|Fifty-Eight|Fifty-Seven|Fifty-Three|Forty-Eight|Forty-Seven|Forty-Three|Ninety-Five|Ninety-Four|Ninety-Nine|One Hundred|Seventy-One|Seventy-Six|Seventy-Two|Sixty-Eight|Sixty-Seven|Sixty-Three|Thirty-Five|Thirty-Four|Thirty-Nine|Twenty-Five|Twenty-Four|Twenty-Nine|Eighty-One|Eighty-Six|Eighty-Two|Fifty-Five|Fifty-Four|Fifty-Nine|Forty-Five|Forty-Four|Forty-Nine|Ninety-One|Ninety-Six|Ninety-Two|Sixty-Five|Sixty-Four|Sixty-Nine|Thirty-One|Thirty-Six|Thirty-Two|Twenty-One|Twenty-Six|Twenty-Two|Fifty-One|Fifty-Six|Fifty-Two|Forty-One|Forty-Six|Forty-Two|Seventeen|Sixty-One|Sixty-Six|Sixty-Two|Eighteen|Fourteen|Nineteen|Thirteen|Fifteen|Seventy|Sixteen|Eighty|Eleven|Ninety|Thirty|Twelve|Twenty|Eight|Fifty|Forty|Seven|Sixty|Three|Five|Four|Nine|One|Six|Ten|Two)(?:\s*[:\-]\s*(.+))?$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex ChapterWrittenRegex();

    [GeneratedRegex(@"^Part\s+(One|Two|Three|Four|Five|Six|Seven|Eight|Nine|Ten)(?:\s*[:\-]\s*(.+))?$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex PartWrittenRegex();

    [GeneratedRegex(@"^(Prologue|Epilogue|Introduction|Foreword|Afterword|Preface)(?:\s*[:\-]\s*(.+))?$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex NamedSectionRegex();

    [GeneratedRegex(@"^(\*{3,}|-{3,}|={3,})$", RegexOptions.Multiline)]
    private static partial Regex DividerRegex();

    private static readonly Dictionary<string, int> WrittenNumberMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["One"] = 1, ["Two"] = 2, ["Three"] = 3, ["Four"] = 4, ["Five"] = 5,
        ["Six"] = 6, ["Seven"] = 7, ["Eight"] = 8, ["Nine"] = 9, ["Ten"] = 10,
        ["Eleven"] = 11, ["Twelve"] = 12, ["Thirteen"] = 13, ["Fourteen"] = 14, ["Fifteen"] = 15,
        ["Sixteen"] = 16, ["Seventeen"] = 17, ["Eighteen"] = 18, ["Nineteen"] = 19, ["Twenty"] = 20,
        ["Twenty-One"] = 21, ["Twenty-Two"] = 22, ["Twenty-Three"] = 23, ["Twenty-Four"] = 24, ["Twenty-Five"] = 25,
        ["Twenty-Six"] = 26, ["Twenty-Seven"] = 27, ["Twenty-Eight"] = 28, ["Twenty-Nine"] = 29, ["Thirty"] = 30,
        ["Thirty-One"] = 31, ["Thirty-Two"] = 32, ["Thirty-Three"] = 33, ["Thirty-Four"] = 34, ["Thirty-Five"] = 35,
        ["Thirty-Six"] = 36, ["Thirty-Seven"] = 37, ["Thirty-Eight"] = 38, ["Thirty-Nine"] = 39, ["Forty"] = 40,
        ["Forty-One"] = 41, ["Forty-Two"] = 42, ["Forty-Three"] = 43, ["Forty-Four"] = 44, ["Forty-Five"] = 45,
        ["Forty-Six"] = 46, ["Forty-Seven"] = 47, ["Forty-Eight"] = 48, ["Forty-Nine"] = 49, ["Fifty"] = 50,
        ["Fifty-One"] = 51, ["Fifty-Two"] = 52, ["Fifty-Three"] = 53, ["Fifty-Four"] = 54, ["Fifty-Five"] = 55,
        ["Fifty-Six"] = 56, ["Fifty-Seven"] = 57, ["Fifty-Eight"] = 58, ["Fifty-Nine"] = 59, ["Sixty"] = 60,
        ["Sixty-One"] = 61, ["Sixty-Two"] = 62, ["Sixty-Three"] = 63, ["Sixty-Four"] = 64, ["Sixty-Five"] = 65,
        ["Sixty-Six"] = 66, ["Sixty-Seven"] = 67, ["Sixty-Eight"] = 68, ["Sixty-Nine"] = 69, ["Seventy"] = 70,
        ["Seventy-One"] = 71, ["Seventy-Two"] = 72, ["Seventy-Three"] = 73, ["Seventy-Four"] = 74, ["Seventy-Five"] = 75,
        ["Seventy-Six"] = 76, ["Seventy-Seven"] = 77, ["Seventy-Eight"] = 78, ["Seventy-Nine"] = 79, ["Eighty"] = 80,
        ["Eighty-One"] = 81, ["Eighty-Two"] = 82, ["Eighty-Three"] = 83, ["Eighty-Four"] = 84, ["Eighty-Five"] = 85,
        ["Eighty-Six"] = 86, ["Eighty-Seven"] = 87, ["Eighty-Eight"] = 88, ["Eighty-Nine"] = 89, ["Ninety"] = 90,
        ["Ninety-One"] = 91, ["Ninety-Two"] = 92, ["Ninety-Three"] = 93, ["Ninety-Four"] = 94, ["Ninety-Five"] = 95,
        ["Ninety-Six"] = 96, ["Ninety-Seven"] = 97, ["Ninety-Eight"] = 98, ["Ninety-Nine"] = 99, ["One Hundred"] = 100
    };

    public IReadOnlyList<DetectedChapter> DetectChapters(string text)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        var chapters = new List<ChapterMatch>();

        // Find all chapter markers
        FindMatches(text, ChapterNumberedRegex(), chapters, isNumbered: true);
        FindMatches(text, ChapterAbbreviationRegex(), chapters, isNumbered: true);
        FindMatches(text, PartNumberedRegex(), chapters, isNumbered: true);
        FindMatches(text, SectionNumberedRegex(), chapters, isNumbered: true);
        FindMatches(text, ChapterWrittenRegex(), chapters, isNumbered: false, isWritten: true);
        FindMatches(text, PartWrittenRegex(), chapters, isNumbered: false, isWritten: true);
        FindMatches(text, NamedSectionRegex(), chapters, isNumbered: false);
        FindMatches(text, DividerRegex(), chapters, isNumbered: false, isDivider: true);

        if (chapters.Count == 0)
            return [];

        // Sort by position
        chapters.Sort((a, b) => a.StartPosition.CompareTo(b.StartPosition));

        // Build DetectedChapter list with end positions and word counts
        var result = new List<DetectedChapter>();
        for (var i = 0; i < chapters.Count; i++)
        {
            var current = chapters[i];
            var endPosition = i < chapters.Count - 1 ? chapters[i + 1].StartPosition : text.Length;
            var chunkText = text[current.StartPosition..endPosition];
            var wordCount = EstimateWordCount(chunkText);

            result.Add(new DetectedChapter
            {
                ChapterNumber = current.ChapterNumber,
                Title = current.Title,
                StartPosition = current.StartPosition,
                EndPosition = endPosition,
                EstimatedWordCount = wordCount
            });
        }

        return result;
    }

    public bool HasChapters(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        // Quick check without full detection
        return ChapterNumberedRegex().IsMatch(text) ||
               ChapterAbbreviationRegex().IsMatch(text) ||
               PartNumberedRegex().IsMatch(text) ||
               SectionNumberedRegex().IsMatch(text) ||
               ChapterWrittenRegex().IsMatch(text) ||
               PartWrittenRegex().IsMatch(text) ||
               NamedSectionRegex().IsMatch(text) ||
               DividerRegex().IsMatch(text);
    }

    private static void FindMatches(
        string text,
        Regex regex,
        List<ChapterMatch> chapters,
        bool isNumbered = false,
        bool isWritten = false,
        bool isDivider = false)
    {
        var matches = regex.Matches(text);
        foreach (Match match in matches)
        {
            var chapterNumber = 0;
            var title = match.Value.Trim();

            if (isNumbered && match.Groups.Count > 1)
            {
                chapterNumber = int.Parse(match.Groups[1].Value);
                // Include subtitle if present
                if (match.Groups.Count > 2 && !string.IsNullOrWhiteSpace(match.Groups[2].Value))
                {
                    title = match.Groups[0].Value.Trim();
                }
            }
            else if (isWritten && match.Groups.Count > 1)
            {
                var writtenNumber = match.Groups[1].Value;
                if (WrittenNumberMap.TryGetValue(writtenNumber, out var number))
                {
                    chapterNumber = number;
                }
            }
            else if (isDivider)
            {
                // Dividers don't have chapter numbers
                chapterNumber = 0;
            }

            chapters.Add(new ChapterMatch
            {
                StartPosition = match.Index,
                ChapterNumber = chapterNumber,
                Title = title
            });
        }
    }

     private static int EstimateWordCount(string text)
     {
         if (string.IsNullOrWhiteSpace(text))
             return 0;

         var wordCount = 0;
         var inWord = false;

         foreach (var c in text)
         {
             var isWhitespace = c is ' ' or '\t' or '\n' or '\r';

             if (!isWhitespace && !inWord)
             {
                 wordCount++;
                 inWord = true;
             }
             else if (isWhitespace)
             {
                 inWord = false;
             }
         }

         return wordCount;
     }

    private class ChapterMatch
    {
        public required int StartPosition { get; init; }
        public required int ChapterNumber { get; init; }
        public required string Title { get; init; }
    }
}
