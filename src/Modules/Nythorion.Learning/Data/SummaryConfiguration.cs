using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nythorion.Learning.Domain;

namespace Nythorion.Learning.Data;

internal sealed class SummaryConfiguration : IEntityTypeConfiguration<Summary>
{
    public void Configure(EntityTypeBuilder<Summary> builder)
    {
        builder.ToTable("summaries");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Content).IsRequired();
    }
}
