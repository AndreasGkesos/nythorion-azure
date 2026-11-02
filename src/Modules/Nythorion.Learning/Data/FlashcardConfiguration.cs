using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nythorion.Learning.Domain;

namespace Nythorion.Learning.Data;

internal sealed class FlashcardConfiguration : IEntityTypeConfiguration<Flashcard>
{
    public void Configure(EntityTypeBuilder<Flashcard> builder)
    {
        builder.ToTable("flashcards");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Question).IsRequired();
        builder.Property(f => f.Answer).IsRequired();
    }
}
