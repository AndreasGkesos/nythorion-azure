using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Learning.Data;

namespace Nythorion.Learning.Features.DeleteFlashcard;

internal sealed class DeleteFlashcardHandler(ILearningDbContext db)
    : IRequestHandler<DeleteFlashcardCommand>
{
    public async Task Handle(DeleteFlashcardCommand request, CancellationToken cancellationToken)
    {
        var flashcard = await db.Flashcards
            .FirstOrDefaultAsync(f => f.Id == request.FlashcardId, cancellationToken)
            ?? throw new InvalidOperationException($"Flashcard {request.FlashcardId} not found.");

        db.Flashcards.Remove(flashcard);
        await db.SaveChangesAsync(cancellationToken);
    }
}
