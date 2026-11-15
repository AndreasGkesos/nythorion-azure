using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nythorion.Learning.Data;
using Nythorion.Learning.Generation;

namespace Nythorion.Learning.Features.GenerateFlashcards;

internal sealed class GenerateFlashcardsHandler(ILearningDbContext db, FlashcardGenerator generator, ILogger<GenerateFlashcardsHandler> logger)
    : IRequestHandler<GenerateFlashcardsCommand, GenerateFlashcardsResult>
{
    public async Task<GenerateFlashcardsResult> Handle(GenerateFlashcardsCommand request, CancellationToken cancellationToken)
    {
        if (!await db.Documents.AnyAsync(d => d.Id == request.DocumentId && d.UserId == request.UserId, cancellationToken))
            throw new KeyNotFoundException($"Document {request.DocumentId} not found.");

        var allChunks = await db.DocumentChunks
            .Where(c => c.DocumentId == request.DocumentId)
            .OrderBy(c => c.ChunkIndex)
            .Select(c => c.Content)
            .ToListAsync(cancellationToken);

        if (allChunks.Count == 0)
            throw new InvalidOperationException($"No chunks found for document {request.DocumentId}.");

        var existingQuestions = await db.Flashcards
            .Where(f => f.DocumentId == request.DocumentId)
            .Select(f => f.Question)
            .ToListAsync(cancellationToken);

        var created = new List<Domain.Flashcard>();

        for (var i = 0; i < request.Count; i++)
        {
            var flashcard = await generator.GenerateAsync(request.DocumentId, allChunks, existingQuestions, cancellationToken);
            if (flashcard is null)
            {
                logger.LogWarning("Flashcard {Index} could not be parsed, skipping.", i + 1);
                continue;
            }

            db.Flashcards.Add(flashcard);
            await db.SaveChangesAsync(cancellationToken);

            existingQuestions.Add(flashcard.Question);
            created.Add(flashcard);
        }

        return new GenerateFlashcardsResult(
            request.DocumentId,
            created.Select(f => new FlashcardDto(f.Id, f.Question, f.Answer)).ToList());
    }
}
