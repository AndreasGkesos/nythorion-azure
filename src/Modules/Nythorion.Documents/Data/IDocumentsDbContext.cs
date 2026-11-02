using Microsoft.EntityFrameworkCore;
using Nythorion.Documents.Domain;

namespace Nythorion.Documents.Data;

public interface IDocumentsDbContext
{
    DbSet<Document> Documents { get; }
    DbSet<DocumentChunk> DocumentChunks { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
