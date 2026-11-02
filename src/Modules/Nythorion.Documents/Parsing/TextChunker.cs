using System.Text.RegularExpressions;

namespace Nythorion.Documents.Parsing;

public static class TextChunker
{
    private const int MaxWordsPerChunk = 400;
    private const int OverlapSentences = 2;
    private const double MinAlphanumericRatio = 0.4;

    // Splits on sentence-ending punctuation followed by whitespace and an uppercase letter or end of string
    private static readonly Regex SentenceSplitter = new(
        @"(?<=[.!?])\s+(?=[A-Z""'\(])|(?<=[.!?])\s*$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    public static IReadOnlyList<string> Chunk(string text)
    {
        var sentences = SentenceSplitter
            .Split(text.Trim())
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();

        var chunks = new List<string>();
        var i = 0;

        while (i < sentences.Count)
        {
            var accumulated = new List<string>();
            var wordCount = 0;
            var j = i;

            while (j < sentences.Count)
            {
                var sentenceWords = CountWords(sentences[j]);
                if (accumulated.Count > 0 && wordCount + sentenceWords > MaxWordsPerChunk)
                    break;

                accumulated.Add(sentences[j]);
                wordCount += sentenceWords;
                j++;
            }

            // If a single sentence exceeds the limit, include it anyway to avoid infinite loop
            if (accumulated.Count == 0)
            {
                accumulated.Add(sentences[j]);
                j++;
            }

            var chunk = string.Join(" ", accumulated);
            if (!IsNoisy(chunk))
                chunks.Add(chunk);

            // Advance by the number of new sentences, keeping last N for overlap
            var newSentences = j - i;
            i += Math.Max(1, newSentences - OverlapSentences);
        }

        return chunks;
    }

    private static int CountWords(string sentence) =>
        sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

    // Matches ToC dot-leaders like ". . . . . 5" or "......... 12"
    private static readonly Regex DotLeaderPattern =
        new(@"(\. ){4,}|\s\.{4,}", RegexOptions.Compiled);

    private static bool IsNoisy(string chunk)
    {
        if (chunk.Length == 0) return true;
        var alphanumericCount = chunk.Count(char.IsLetterOrDigit);
        if ((double)alphanumericCount / chunk.Length < MinAlphanumericRatio) return true;

        var dotLeaderMatches = DotLeaderPattern.Matches(chunk);
        if (dotLeaderMatches.Count >= 3) return true;

        return false;
    }
}
