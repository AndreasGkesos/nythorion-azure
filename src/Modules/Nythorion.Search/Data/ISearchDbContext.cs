using Microsoft.EntityFrameworkCore;
using Nythorion.Documents.Domain;

namespace Nythorion.Search.Data;

public interface ISearchDbContext
{
    DbSet<Document> Documents { get; }
    DbSet<DocumentChunk> DocumentChunks { get; }
}
