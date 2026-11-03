using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nythorion.Documents.Domain;

namespace Nythorion.Documents.Data;

public class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    // Dimension of the text-embedding-3-small embedding model
    private const int EmbeddingDimension = 1536;

    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("document_chunks");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Content).IsRequired();
        builder.Property(c => c.Embedding).HasColumnType($"vector({EmbeddingDimension})");
    }
}
