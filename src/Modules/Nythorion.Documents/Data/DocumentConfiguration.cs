using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nythorion.Documents.Domain;

namespace Nythorion.Documents.Data;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.UserId).IsRequired().HasMaxLength(450);
        builder.Property(d => d.DisplayName).IsRequired().HasMaxLength(255);
        builder.Property(d => d.FileName).IsRequired().HasMaxLength(500);
        builder.Property(d => d.ContentType).IsRequired().HasMaxLength(100);
        builder.HasMany(d => d.Chunks).WithOne(c => c.Document).HasForeignKey(c => c.DocumentId);
    }
}
