using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nythorion.Embeddings.OllamaClient;
using Nythorion.Search.Data;
using Nythorion.Search.OllamaClient;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Nythorion.Search.Features.Chat;

internal sealed class ChatHandler(ISearchDbContext db, OllamaEmbeddingClient embeddingClient, OllamaLlmClient llmClient, ILogger<ChatHandler> logger)
    : IRequestHandler<ChatQuery, ChatResult>
{
    private const int TopK = 8;

    private const string AnswerSystemPrompt = """
        You are a helpful assistant that answers questions based strictly on the provided document excerpts.
        You are in a conversation — use previous messages for context, but only answer from the excerpts below.
        If the answer cannot be found in the excerpts, say so clearly.
        Do not make up information. Answer in plain prose only — no headers, no bullet points, no markdown.
        """;

    public async Task<ChatResult> Handle(ChatQuery request, CancellationToken cancellationToken)
    {
        var lastUserMessage = request.Messages.LastOrDefault(m => m.Role == "user")?.Content ?? string.Empty;

        // Only rewrite if there is prior conversation history
        var searchQuery = request.Messages.Count > 1
            ? await RewriteQueryAsync(request.Messages, lastUserMessage, cancellationToken)
            : lastUserMessage;

        logger.LogInformation("Chat search query: {Query}", searchQuery);

        var queryEmbedding = await embeddingClient.GetEmbeddingAsync(searchQuery, cancellationToken);
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
            return new ChatResult("I could not find any relevant information in your documents.", []);

        var context = string.Join("\n\n", chunks.Select((c, i) => $"[{i + 1}] {c.Content}"));
        var sources = chunks.Select(c => c.DocumentId.ToString()).Distinct().ToList();

        var history = request.Messages.ToList();
        var lastIndex = history.FindLastIndex(m => m.Role == "user");
        history[lastIndex] = new ChatMessage("user", $"Document excerpts:\n{context}\n\nQuestion: {history[lastIndex].Content}");

        var answer = await llmClient.ChatAsync(AnswerSystemPrompt, history, cancellationToken);

        return new ChatResult(answer, sources);
    }

    private async Task<string> RewriteQueryAsync(List<ChatMessage> messages, string lastUserMessage, CancellationToken cancellationToken)
    {
        const string systemPrompt = """
            You are a search query rewriter. Given a conversation history and the latest user message,
            rewrite the latest message into a single, self-contained search query that captures the full
            context needed to find relevant information. Output ONLY the rewritten query — no explanation,
            no punctuation at the end, no extra text.
            """;

        var history = string.Join("\n", messages.SkipLast(1).Select(m => $"{m.Role}: {m.Content}"));
        var userPrompt = $"Conversation so far:\n{history}\n\nLatest message: {lastUserMessage}\n\nRewritten search query:";

        var rewritten = await llmClient.GenerateAsync(systemPrompt, userPrompt, cancellationToken);
        return string.IsNullOrWhiteSpace(rewritten) ? lastUserMessage : rewritten.Trim();
    }
}
