using FluentAssertions;
using Nythorion.Documents.Parsing;

namespace Nythorion.Documents.Tests;

public class TextChunkerTests
{
    [Fact]
    public void Chunk_EmptyString_ReturnsNoChunks()
    {
        var result = TextChunker.Chunk("");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Chunk_WhitespaceOnly_ReturnsNoChunks()
    {
        var result = TextChunker.Chunk("   \n\n  ");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Chunk_SingleSentence_ReturnsSingleChunk()
    {
        var text = "This is one complete sentence with enough words.";

        var result = TextChunker.Chunk(text);

        result.Should().ContainSingle();
    }

    [Fact]
    public void Chunk_FirstChunk_ContainsOpeningSentences()
    {
        var text = "First sentence. Second sentence. Third sentence.";

        var result = TextChunker.Chunk(text);

        result.Should().NotBeEmpty();
        result[0].Should().Contain("First sentence");
    }

    [Fact]
    public void Chunk_LongText_SplitsIntoMultipleChunks()
    {
        var sentence = "This is a sentence with exactly ten words total here. ";
        var text = string.Concat(Enumerable.Repeat(sentence, 60));

        var result = TextChunker.Chunk(text);

        result.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void Chunk_MultipleChunks_EachWithinWordLimit()
    {
        var sentence = "This is a sentence with exactly ten words total here. ";
        var text = string.Concat(Enumerable.Repeat(sentence, 60));

        var result = TextChunker.Chunk(text);

        foreach (var chunk in result)
        {
            var wordCount = chunk.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            wordCount.Should().BeLessOrEqualTo(420, "overlap sentences may push slightly past 400");
        }
    }

    [Fact]
    public void Chunk_WithOverlap_ConsecutiveChunksShareSentences()
    {
        var sentences = Enumerable.Range(1, 50)
            .Select(i => $"Sentence number {i} contains some words to fill space properly.")
            .ToList();
        var text = string.Join(" ", sentences);

        var result = TextChunker.Chunk(text);

        result.Should().HaveCountGreaterThan(1);
        var firstChunkLastSentence = result[0].Split('.').Where(s => s.Trim().Length > 0).Last().Trim();
        result[1].Should().Contain(firstChunkLastSentence[..Math.Min(20, firstChunkLastSentence.Length)]);
    }

    [Fact]
    public void Chunk_NoisyTocChunk_IsFiltered()
    {
        var toc = "Introduction . . . . . . . . . . . 1\n" +
                  "Chapter One . . . . . . . . . . . 5\n" +
                  "Chapter Two . . . . . . . . . . . 12\n" +
                  "Conclusion . . . . . . . . . . . 20";

        var result = TextChunker.Chunk(toc);

        result.Should().BeEmpty("ToC dot-leader content should be filtered as noisy");
    }

    [Fact]
    public void Chunk_LowAlphanumericRatio_IsFiltered()
    {
        // Text that is mostly symbols/punctuation
        var noisy = "--- === *** ||| ... --- === *** ||| ... --- === *** ||| ...";

        var result = TextChunker.Chunk(noisy);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Chunk_NormalParagraph_IsNotFiltered()
    {
        var text = "The microkernel architecture is a flexible design pattern. " +
                   "It separates core functionality from optional extensions. " +
                   "Plug-ins can be added without modifying the core system.";

        var result = TextChunker.Chunk(text);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public void Chunk_EachChunk_EndsOnSentenceBoundary()
    {
        var text = "First complete sentence here. Second complete sentence here. Third complete sentence here.";

        var result = TextChunker.Chunk(text);

        foreach (var chunk in result)
        {
            var trimmed = chunk.TrimEnd();
            trimmed.Last().Should().BeOneOf('.', '!', '?', '"', '\'');
        }
    }

    [Fact]
    public void Chunk_ExclamationAndQuestion_AllContentPresentInFirstChunk()
    {
        var text = "Is this working? Yes it is! This confirms splitting works. Another sentence follows.";

        var result = TextChunker.Chunk(text);

        result.Should().NotBeEmpty();
        result[0].Should().Contain("Is this working?");
        result[0].Should().Contain("Yes it is!");
    }

    [Fact]
    public void Chunk_SingleSentenceExceedingWordLimit_IsIncludedAnyway()
    {
        var words = string.Join(" ", Enumerable.Repeat("word", 450));
        var text = words + ".";

        var result = TextChunker.Chunk(text);

        result.Should().NotBeEmpty("oversized single sentence must still be included");
        result[0].Should().Contain("word");
    }

    [Fact]
    public void Chunk_DotLeaderBoundary_ExactlyThreeMatches_IsFiltered()
    {
        var text = "Chapter One . . . . . 1\n" +
                   "Chapter Two . . . . . 5\n" +
                   "Chapter Three . . . . . 10";

        var result = TextChunker.Chunk(text);

        result.Should().BeEmpty("three dot-leader matches should be filtered");
    }

    [Fact]
    public void Chunk_DotLeaderBoundary_TwoMatches_IsNotFiltered()
    {
        var text = "Chapter One . . . . . 1\n" +
                   "Chapter Two . . . . . 5\n" +
                   "This is a real sentence with normal content and enough alphanumeric characters.";

        var result = TextChunker.Chunk(text);

        result.Should().NotBeEmpty("two dot-leader matches is below the filter threshold");
    }

    [Fact]
    public void Chunk_AllTextSurvivesChunking_NothingDropped()
    {
        // Every sentence should appear in at least one chunk
        var sentences = Enumerable.Range(1, 30)
            .Select(i => $"Unique sentence identifier alpha{i} beta{i} gamma{i} delta{i} epsilon{i}.")
            .ToList();
        var text = string.Join(" ", sentences);

        var result = TextChunker.Chunk(text);
        var combined = string.Join(" ", result);

        foreach (var sentence in sentences)
        {
            var uniqueToken = sentence.Split(' ')[3]; // e.g. "alpha1"
            combined.Should().Contain(uniqueToken, $"token from sentence should appear in at least one chunk");
        }
    }

    [Fact]
    public void Chunk_QuotedSentenceEnding_AllSentencesSurvive()
    {
        var text = "He said \"This is a quote.\" She replied with a nod. The meeting ended there.";

        var result = TextChunker.Chunk(text);

        var combined = string.Join(" ", result);
        combined.Should().Contain("He said");
        combined.Should().Contain("She replied with a nod");
        combined.Should().Contain("The meeting ended there");
    }

    [Fact]
    public void Chunk_LastChunk_ContainsLastSentence()
    {
        var sentences = Enumerable.Range(1, 50)
            .Select(i => $"Sentence number {i} contains enough words to fill the chunk properly.")
            .ToList();
        var text = string.Join(" ", sentences);

        var result = TextChunker.Chunk(text);

        var allText = string.Join(" ", result);
        allText.Should().Contain("Sentence number 50");
    }
}
