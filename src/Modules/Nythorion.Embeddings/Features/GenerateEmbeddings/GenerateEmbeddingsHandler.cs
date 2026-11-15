using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Nythorion.Embeddings.Data;
using Pgvector;

namespace Nythorion.Embeddings.Features.GenerateEmbeddings;

public class GenerateEmbeddingsHandler(IEmbeddingsDbContext db, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    : IRequestHandler<GenerateEmbeddingsCommand, GenerateEmbeddingsResult>
{
    private const int MaxChunkLength = 6000;
    private const int BatchSize = 64;

    public async Task<GenerateEmbeddingsResult> Handle(GenerateEmbeddingsCommand request, CancellationToken cancellationToken)
    {
        var chunks = await db.DocumentChunks
            .Where(c => c.DocumentId == request.DocumentId && c.Embedding == null)
            .ToListAsync(cancellationToken);

        foreach (var batch in chunks.Chunk(BatchSize))
        {
            var texts = batch.Select(c => c.Content.Length > MaxChunkLength ? c.Content[..MaxChunkLength] : c.Content).ToList();
            var embeddings = await embeddingGenerator.GenerateAsync(texts, cancellationToken: cancellationToken);

            for (var i = 0; i < batch.Length; i++)
                batch[i].Embedding = new Vector(embeddings[i].Vector);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new GenerateEmbeddingsResult(request.DocumentId, chunks.Count);
    }
}
