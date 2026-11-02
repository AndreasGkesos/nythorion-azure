using MediatR;
using Nythorion.Learning.Features.GetNotes;

namespace Nythorion.Learning.Features.AddNote;

public sealed record AddNoteCommand(Guid DocumentId, string UserId, string Content) : IRequest<NoteResult>;
