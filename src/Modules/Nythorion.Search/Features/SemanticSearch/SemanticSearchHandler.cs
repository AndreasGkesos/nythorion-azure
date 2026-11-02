using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Embeddings.OllamaClient;
using Nythorion.Search.Data;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Nythorion.Search.Features.SemanticSearch;

internal sealed class SemanticSearchHandler(ISearchDbContext db, OllamaEmbeddingClient ollamaClient)
    : IRequestHandler<SemanticSearchQuery, List<SemanticSearchResult>>
{
    public async Task<List<SemanticSearchResult>> Handle(SemanticSearchQuery request, CancellationToken cancellationToken)
    {
        var queryEmbedding = await ollamaClient.GetEmbeddingAsync(request.Text, cancellationToken);
        var queryVector = new Vector(queryEmbedding);

        var query = db.DocumentChunks.Where(c => c.Embedding != null && c.Document.UserId == request.UserId);

        if (request.DocumentId.HasValue)
            query = query.Where(c => c.DocumentId == request.DocumentId.Value);

        var results = await query
            .Join(db.Documents, c => c.DocumentId, d => d.Id, (c, d) => new { Chunk = c, d.FileName })
            .OrderBy(x => x.Chunk.Embedding!.CosineDistance(queryVector))
            .Take(request.TopK)
            .Select(x => new SemanticSearchResult(
                x.Chunk.DocumentId,
                x.FileName,
                x.Chunk.Id,
                x.Chunk.ChunkIndex,
                x.Chunk.Content,
                (double)x.Chunk.Embedding!.CosineDistance(queryVector)))
            .ToListAsync(cancellationToken);

        return results;
    }
}
