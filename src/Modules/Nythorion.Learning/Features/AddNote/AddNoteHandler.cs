using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Learning.Data;
using Nythorion.Learning.Domain;
using Nythorion.Learning.Features.GetNotes;

namespace Nythorion.Learning.Features.AddNote;

internal sealed class AddNoteHandler(ILearningDbContext db)
    : IRequestHandler<AddNoteCommand, NoteResult>
{
    public async Task<NoteResult> Handle(AddNoteCommand request, CancellationToken cancellationToken)
    {
        if (!await db.Documents.AnyAsync(d => d.Id == request.DocumentId && d.UserId == request.UserId, cancellationToken))
            throw new KeyNotFoundException($"Document {request.DocumentId} not found.");

        var note = Note.Create(request.DocumentId, request.Content);
        db.Notes.Add(note);
        await db.SaveChangesAsync(cancellationToken);
        return new NoteResult(note.Id, note.Content, note.CreatedAt, note.UpdatedAt);
    }
}
