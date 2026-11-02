using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Embeddings.OllamaClient;
using Nythorion.Search.Data;
using Nythorion.Search.OllamaClient;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Nythorion.Search.Features.Ask;

internal sealed class AskHandler(ISearchDbContext db, OllamaEmbeddingClient embeddingClient, OllamaLlmClient llmClient)
    : IRequestHandler<AskQuery, AskResult>
{
    private const int TopK = 8;

    public async Task<AskResult> Handle(AskQuery request, CancellationToken cancellationToken)
    {
        var queryEmbedding = await embeddingClient.GetEmbeddingAsync(request.Question, cancellationToken);
        var queryVector = new Vector(queryEmbedding);

        var query = db.DocumentChunks.Where(c => c.Embedding != null);

        if (request.DocumentId.HasValue)
            query = query.Where(c => c.DocumentId == request.DocumentId.Value);

        var chunks = await query
            .OrderBy(c => c.Embedding!.CosineDistance(queryVector))
            .Take(TopK)
            .Select(c => new { c.DocumentId, c.Content })
            .ToListAsync(cancellationToken);

        if (chunks.Count == 0)
            return new AskResult("I could not find any relevant information in your documents.", []);

        var context = string.Join("\n\n", chunks.Select((c, i) => $"[{i + 1}] {c.Content}"));
        var sources = chunks.Select(c => c.DocumentId.ToString()).Distinct().ToList();

        var systemPrompt = """
            You are a helpful assistant that answers questions based strictly on the provided document excerpts.
            If the answer cannot be found in the excerpts, say so clearly.
            Do not make up information. Answer in plain prose only — no headers, no bullet points, no lists, no markdown.
            """;

        var userPrompt = $"""
            Document excerpts:
            {context}

            Question: {request.Question}

            Answer in plain prose only. Do not use bullet points, numbered lists, headers, or any markdown formatting.
            """;

        var answer = await llmClient.GenerateAsync(systemPrompt, userPrompt, cancellationToken);

        return new AskResult(answer, sources);
    }
}
