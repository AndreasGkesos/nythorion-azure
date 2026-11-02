using Microsoft.EntityFrameworkCore;
using Nythorion.Documents.Domain;

namespace Nythorion.Embeddings.Data;

public interface IEmbeddingsDbContext
{
    DbSet<DocumentChunk> DocumentChunks { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
