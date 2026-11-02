using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Learning.Data;

namespace Nythorion.Learning.Features.GetNotes;

internal sealed class GetNotesHandler(ILearningDbContext db)
    : IRequestHandler<GetNotesQuery, List<NoteResult>>
{
    public async Task<List<NoteResult>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
    {
        return await db.Notes
            .Where(n => n.DocumentId == request.DocumentId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NoteResult(n.Id, n.Content, n.CreatedAt, n.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
