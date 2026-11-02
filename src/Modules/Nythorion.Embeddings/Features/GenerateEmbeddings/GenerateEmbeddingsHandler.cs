using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Embeddings.Data;
using Nythorion.Embeddings.OllamaClient;
using Pgvector;

namespace Nythorion.Embeddings.Features.GenerateEmbeddings;

public class GenerateEmbeddingsHandler(IEmbeddingsDbContext db, OllamaEmbeddingClient ollamaClient)
    : IRequestHandler<GenerateEmbeddingsCommand, GenerateEmbeddingsResult>
{
    public async Task<GenerateEmbeddingsResult> Handle(GenerateEmbeddingsCommand request, CancellationToken cancellationToken)
    {
        var chunks = await db.DocumentChunks
            .Where(c => c.DocumentId == request.DocumentId && c.Embedding == null)
            .ToListAsync(cancellationToken);

        foreach (var chunk in chunks)
        {
            var text = chunk.Content.Length > 6000 ? chunk.Content[..6000] : chunk.Content;
            var floats = await ollamaClient.GetEmbeddingAsync(text, cancellationToken);
            chunk.Embedding = new Vector(floats);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new GenerateEmbeddingsResult(request.DocumentId, chunks.Count);
    }
}
