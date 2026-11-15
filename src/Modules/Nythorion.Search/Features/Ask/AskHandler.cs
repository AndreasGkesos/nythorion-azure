using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Nythorion.Search.Data;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Nythorion.Search.Features.Ask;

internal sealed class AskHandler(ISearchDbContext db, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, IChatClient chatClient)
    : IRequestHandler<AskQuery, AskResult>
{
    private const int TopK = 8;

    public async Task<AskResult> Handle(AskQuery request, CancellationToken cancellationToken)
    {
        var queryEmbedding = await embeddingGenerator.GenerateAsync(request.Question, cancellationToken: cancellationToken);
        var queryVector = new Vector(queryEmbedding.Vector);

        var query = db.DocumentChunks.Where(c => c.Embedding != null && c.Document.UserId == request.UserId);

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

        var response = await chatClient.GetResponseAsync(
            [new ChatMessage(ChatRole.System, systemPrompt), new ChatMessage(ChatRole.User, userPrompt)],
            cancellationToken: cancellationToken);

        return new AskResult(response.Text, sources);
    }
}
