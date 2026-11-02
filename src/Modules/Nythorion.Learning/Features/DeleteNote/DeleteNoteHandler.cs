using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Learning.Data;

namespace Nythorion.Learning.Features.DeleteNote;

internal sealed class DeleteNoteHandler(ILearningDbContext db)
    : IRequestHandler<DeleteNoteCommand>
{
    public async Task Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        await db.Notes.Where(n => n.Id == request.NoteId).ExecuteDeleteAsync(cancellationToken);
    }
}
