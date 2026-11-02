using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nythorion.Learning.Domain;

namespace Nythorion.Learning.Data;

internal sealed class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("quiz_questions");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Question).IsRequired();
        builder.Property(q => q.Options)
            .HasColumnType("text[]")
            .IsRequired();
    }
}
