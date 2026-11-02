using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nythorion.Learning.Data;
using Nythorion.Learning.Domain;

namespace Nythorion.Learning.OllamaClient;

internal sealed class FlashcardGenerator(ILearningDbContext db, OllamaLlmClient llmClient, ILogger<FlashcardGenerator> logger)
{
    private const string SystemPrompt = """
        You are a flashcard generator. Create 1 flashcard from the provided text.
        The question must be clear and complete — someone reading it with no context should understand what is being asked.
        The answer must be a full sentence explanation.
        Good example: {"question":"What communication mechanism do microservices use?","answer":"Microservices communicate via lightweight mechanisms such as HTTP or message queues."}
        Bad example: {"question":"What is communicationMechanism?","answer":"HTTP"}
        Output ONLY a valid JSON object with EXACTLY these two fields: "question" and "answer". No extra text, no markdown.
        """;

    public async Task<Flashcard?> GenerateAsync(
        Guid documentId,
        List<string> allChunks,
        List<string> existingQuestions,
        CancellationToken cancellationToken)
    {
        // Pick a random window of 5 chunks so each call covers a different part of the document
        var window = allChunks
            .OrderBy(_ => Random.Shared.Next())
            .Take(5)
            .ToList();

        var text = string.Join("\n\n", window);
        var existingList = existingQuestions.Count > 0
            ? "Do NOT duplicate these existing questions:\n" + string.Join("\n", existingQuestions.Select((q, i) => $"{i + 1}. {q}"))
            : string.Empty;

        var userPrompt = $"Generate 1 flashcard from this text. {existingList}\n\nText:\n{text}";

        var json = await llmClient.GenerateAsync(SystemPrompt, userPrompt, cancellationToken);
        logger.LogInformation("Flashcard raw response: {Response}", json);

        return Parse(json, documentId, existingQuestions);
    }

    private Flashcard? Parse(string json, Guid documentId, List<string> existingQuestions)
    {
        json = json.Trim();
        var start = json.IndexOf('{');
        var end = json.LastIndexOf('}');
        if (start < 0 || end <= start) { logger.LogWarning("Parse: no JSON object found in response."); return null; }
        json = json[start..(end + 1)];

        try
        {
            var parsed = JsonSerializer.Deserialize<FlashcardItem>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed?.Question is null || parsed.Answer is null) { logger.LogWarning("Parse: question or answer is null."); return null; }

            if (parsed.Question.Split(' ').Length < 4 || parsed.Answer.Split(' ').Length < 4)
            {
                logger.LogWarning("Parse: rejected — question or answer too short. Q={Question} A={Answer}", parsed.Question, parsed.Answer);
                return null;
            }

            var newWords = Words(parsed.Question);
            if (existingQuestions.Any(q => WordOverlap(newWords, Words(q)) > 0.65))
            {
                logger.LogWarning("Parse: rejected — too similar to an existing question.");
                return null;
            }

            return Flashcard.Create(documentId, parsed.Question, parsed.Answer);
        }
        catch (JsonException ex)
        {
            logger.LogWarning("Parse: JSON exception — {Message}", ex.Message);
            return null;
        }
    }

    private static readonly HashSet<string> StopWords =
    [
        "what", "is", "are", "the", "a", "an", "of", "in", "to", "and", "or",
        "how", "why", "when", "where", "which", "do", "does", "did", "be",
        "between", "for", "on", "at", "by", "with", "from", "that", "this"
    ];

    private static HashSet<string> Words(string text) =>
        text.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !StopWords.Contains(w))
            .ToHashSet();

    private static double WordOverlap(HashSet<string> a, HashSet<string> b)
    {
        var intersection = a.Count(b.Contains);
        return (double)intersection / Math.Max(a.Count, b.Count);
    }

    private sealed record FlashcardItem(string? Question, string? Answer);
}
