using Microsoft.EntityFrameworkCore;
using Nythorion.Documents.Data;
using Nythorion.Documents.Domain;
using Nythorion.Embeddings.Data;
using Nythorion.Learning.Data;
using Nythorion.Learning.Domain;
using Nythorion.Search.Data;

namespace Nythorion.Api.Data;

public class NythorionDbContext(DbContextOptions<NythorionDbContext> options)
    : DbContext(options), IDocumentsDbContext, IEmbeddingsDbContext, ISearchDbContext, ILearningDbContext
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
    public DbSet<Summary> Summaries => Set<Summary>();
    public DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Nythorion.Documents.DocumentsModule).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Nythorion.Learning.LearningModule).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
