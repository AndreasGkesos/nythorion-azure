using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Learning.Data;
using Nythorion.Learning.Features.GenerateFlashcards;

namespace Nythorion.Learning.Features.GetFlashcards;

internal sealed class GetFlashcardsHandler(ILearningDbContext db)
    : IRequestHandler<GetFlashcardsQuery, List<FlashcardDto>>
{
    public async Task<List<FlashcardDto>> Handle(GetFlashcardsQuery request, CancellationToken cancellationToken)
    {
        return await db.Flashcards
            .Where(f => f.DocumentId == request.DocumentId)
            .Select(f => new FlashcardDto(f.Id, f.Question, f.Answer))
            .ToListAsync(cancellationToken);
    }
}
