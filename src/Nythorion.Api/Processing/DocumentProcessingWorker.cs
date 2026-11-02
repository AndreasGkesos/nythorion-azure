using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nythorion.Documents.Domain;
using Nythorion.Embeddings.Features.GenerateEmbeddings;
using Nythorion.Learning.Features.GenerateFlashcards;
using Nythorion.Learning.Features.GenerateQuiz;
using Nythorion.Learning.Features.GenerateSummary;
using Nythorion.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Nythorion.Api.Processing;

public sealed class DocumentProcessingWorker(
    DocumentProcessingChannel channel,
    IServiceScopeFactory scopeFactory,
    ILogger<DocumentProcessingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var documentId in channel.Reader.ReadAllAsync(stoppingToken))
        {
            await ProcessDocumentAsync(documentId, stoppingToken);
        }
    }

    private async Task ProcessDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<NythorionDbContext>();

        try
        {
            var document = await db.Documents.FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);
            if (document is null)
            {
                logger.LogWarning("Document {DocumentId} not found, skipping processing.", documentId);
                return;
            }
            var userId = document.UserId;

            await SetStatus(db, documentId, DocumentStatus.Processing, cancellationToken);

            await mediator.Send(new GenerateEmbeddingsCommand(documentId), cancellationToken);

            var chunkCount = await db.Set<Nythorion.Documents.Domain.DocumentChunk>()
                .CountAsync(c => c.DocumentId == documentId, cancellationToken);

            var flashcardCount = Math.Clamp(chunkCount / 5, 2, 10);
            var quizCount = Math.Clamp(chunkCount / 10, 1, 5);

            await RunStep("summary", () => mediator.Send(new GenerateSummaryCommand(documentId, userId), cancellationToken));
            await RunStep("flashcards", () => mediator.Send(new GenerateFlashcardsCommand(documentId, userId, Count: flashcardCount), cancellationToken));
            await RunStep("quiz", () => mediator.Send(new GenerateQuizCommand(documentId, userId, Count: quizCount), cancellationToken));

            db.ChangeTracker.Clear();
            await SetStatus(db, documentId, DocumentStatus.Ready, cancellationToken);

            logger.LogInformation("Document {DocumentId} processed successfully.", documentId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process document {DocumentId}.", documentId);
            db.ChangeTracker.Clear();
            await SetStatus(db, documentId, DocumentStatus.Failed, cancellationToken);
        }
    }

    private async Task RunStep(string stepName, Func<Task> step)
    {
        try
        {
            await step();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Step '{Step}' failed but processing will continue.", stepName);
        }
    }

    private static async Task SetStatus(NythorionDbContext db, Guid documentId, DocumentStatus status, CancellationToken cancellationToken)
    {
        var document = await db.Documents.FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);
        if (document is null) return;
        document.SetStatus(status);
        await db.SaveChangesAsync(cancellationToken);
    }
}
