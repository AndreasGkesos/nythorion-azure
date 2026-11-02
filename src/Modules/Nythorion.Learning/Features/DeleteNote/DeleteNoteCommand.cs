using MediatR;

namespace Nythorion.Learning.Features.DeleteNote;

public sealed record DeleteNoteCommand(Guid NoteId) : IRequest;
