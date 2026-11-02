using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Learning.Data;
using Nythorion.Learning.Domain;
using Nythorion.Learning.OllamaClient;

namespace Nythorion.Learning.Features.GenerateSummary;

internal sealed class GenerateSummaryHandler(ILearningDbContext db, OllamaLlmClient llmClient)
    : IRequestHandler<GenerateSummaryCommand, GenerateSummaryResult>
{
    private const int ChunksPerBatch = 10;

    public async Task<GenerateSummaryResult> Handle(GenerateSummaryCommand request, CancellationToken cancellationToken)
    {
        if (!await db.Documents.AnyAsync(d => d.Id == request.DocumentId && d.UserId == request.UserId, cancellationToken))
            throw new KeyNotFoundException($"Document {request.DocumentId} not found.");

        var allChunks = await db.DocumentChunks
            .Where(c => c.DocumentId == request.DocumentId)
            .OrderBy(c => c.ChunkIndex)
            .Select(c => c.Content)
            .ToListAsync(cancellationToken);

        if (allChunks.Count == 0)
            throw new InvalidOperationException($"No chunks found for document {request.DocumentId}.");

        var batchSummaries = await MapAsync(allChunks, cancellationToken);
        var content = await ReduceAsync(batchSummaries, cancellationToken);

        var existing = await db.Summaries
            .FirstOrDefaultAsync(s => s.DocumentId == request.DocumentId, cancellationToken);
        if (existing is not null)
            db.Summaries.Remove(existing);

        var summary = Summary.Create(request.DocumentId, content);
        db.Summaries.Add(summary);
        await db.SaveChangesAsync(cancellationToken);

        return new GenerateSummaryResult(summary.Id, summary.Content);
    }

    private async Task<List<string>> MapAsync(List<string> chunks, CancellationToken cancellationToken)
    {
        const string systemPrompt = """
            You are a document summarizer. Summarize only what the user provides. Plain prose only — no headers, no bullets, no markdown.
            """;

        var results = new List<string>();

        for (var i = 0; i < chunks.Count; i += ChunksPerBatch)
        {
            var batch = chunks.Skip(i).Take(ChunksPerBatch).ToList();
            var batchText = string.Join("\n\n", batch);

            var userPrompt = $"""
                Summarize the following passage in 2 to 3 sentences of plain prose. Only use information from the passage below.

                {batchText}
                """;

            var result = await llmClient.GenerateAsync(systemPrompt, userPrompt, cancellationToken, jsonFormat: false);
            results.Add(result.Trim());
        }

        return results;
    }

    private async Task<string> ReduceAsync(List<string> batchSummaries, CancellationToken cancellationToken)
    {
        var combinedSummaries = string.Join("\n\n", batchSummaries);

        const string systemPrompt = """
            You are a document summarizer. Synthesize the provided passage notes into a single cohesive summary. Plain prose only — no headers, no bullets, no markdown.
            """;

        var userPrompt = $"""
            Below are notes summarizing different sections of a document:

            {combinedSummaries}

            Write a detailed plain prose summary of the full document in 8 to 12 sentences. Cover the main topics, key arguments, important concepts, and conclusions. Do not repeat yourself.
            """;

        var result = await llmClient.GenerateAsync(systemPrompt, userPrompt, cancellationToken, jsonFormat: false);
        return result.Trim();
    }
}
