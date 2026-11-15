using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Nythorion.Search.Data;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Nythorion.Search.Features.Chat;

internal sealed class ChatHandler(ISearchDbContext db, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, IChatClient chatClient, ILogger<ChatHandler> logger)
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

        var queryEmbedding = await embeddingGenerator.GenerateAsync(searchQuery, cancellationToken: cancellationToken);
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
            return new ChatResult("I could not find any relevant information in your documents.", []);

        var context = string.Join("\n\n", chunks.Select((c, i) => $"[{i + 1}] {c.Content}"));
        var sources = chunks.Select(c => c.DocumentId.ToString()).Distinct().ToList();

        var history = request.Messages.ToList();
        var lastIndex = history.FindLastIndex(m => m.Role == "user");
        history[lastIndex] = new ChatTurn("user", $"Document excerpts:\n{context}\n\nQuestion: {history[lastIndex].Content}");

        var messages = history
            .Select(turn => new ChatMessage(new ChatRole(turn.Role), turn.Content))
            .Prepend(new ChatMessage(ChatRole.System, AnswerSystemPrompt))
            .ToList();

        var response = await chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);

        return new ChatResult(response.Text, sources);
    }

    private async Task<string> RewriteQueryAsync(List<ChatTurn> messages, string lastUserMessage, CancellationToken cancellationToken)
    {
        const string systemPrompt = """
            You are a search query rewriter. Given a conversation history and the latest user message,
            rewrite the latest message into a single, self-contained search query that captures the full
            context needed to find relevant information. Output ONLY the rewritten query — no explanation,
            no punctuation at the end, no extra text.
            """;

        var history = string.Join("\n", messages.SkipLast(1).Select(m => $"{m.Role}: {m.Content}"));
        var userPrompt = $"Conversation so far:\n{history}\n\nLatest message: {lastUserMessage}\n\nRewritten search query:";

        var response = await chatClient.GetResponseAsync(
            [new ChatMessage(ChatRole.System, systemPrompt), new ChatMessage(ChatRole.User, userPrompt)],
            cancellationToken: cancellationToken);

        return string.IsNullOrWhiteSpace(response.Text) ? lastUserMessage : response.Text.Trim();
    }
}
