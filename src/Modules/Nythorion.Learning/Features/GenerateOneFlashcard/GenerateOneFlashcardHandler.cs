using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Learning.Data;
using Nythorion.Learning.Features.GenerateFlashcards;
using Nythorion.Learning.Generation;

namespace Nythorion.Learning.Features.GenerateOneFlashcard;

internal sealed class GenerateOneFlashcardHandler(ILearningDbContext db, FlashcardGenerator generator)
    : IRequestHandler<GenerateOneFlashcardCommand, FlashcardDto>
{
    public async Task<FlashcardDto> Handle(GenerateOneFlashcardCommand request, CancellationToken cancellationToken)
    {
        if (!await db.Documents.AnyAsync(d => d.Id == request.DocumentId && d.UserId == request.UserId, cancellationToken))
            throw new KeyNotFoundException($"Document {request.DocumentId} not found.");

        var allChunks = await db.DocumentChunks
            .Where(c => c.DocumentId == request.DocumentId)
            .OrderBy(c => c.ChunkIndex)
            .Select(c => c.Content)
            .ToListAsync(cancellationToken);

        var existingQuestions = await db.Flashcards
            .Where(f => f.DocumentId == request.DocumentId)
            .Select(f => f.Question)
            .ToListAsync(cancellationToken);

        var flashcard = await generator.GenerateAsync(request.DocumentId, allChunks, existingQuestions, cancellationToken)
            ?? throw new InvalidOperationException("Could not generate a valid flashcard from the document.");

        db.Flashcards.Add(flashcard);
        await db.SaveChangesAsync(cancellationToken);

        return new FlashcardDto(flashcard.Id, flashcard.Question, flashcard.Answer);
    }
}
