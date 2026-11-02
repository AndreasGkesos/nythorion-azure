using Microsoft.EntityFrameworkCore;
using Nythorion.Documents.Domain;
using Nythorion.Learning.Domain;

namespace Nythorion.Learning.Data;

public interface ILearningDbContext
{
    DbSet<Document> Documents { get; }
    DbSet<DocumentChunk> DocumentChunks { get; }
    DbSet<Summary> Summaries { get; }
    DbSet<Flashcard> Flashcards { get; }
    DbSet<QuizQuestion> QuizQuestions { get; }
    DbSet<Note> Notes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
