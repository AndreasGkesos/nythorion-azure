using MediatR;

namespace Nythorion.Learning.Features.GetNotes;

public sealed record GetNotesQuery(Guid DocumentId) : IRequest<List<NoteResult>>;
public record NoteResult(Guid Id, string Content, DateTime CreatedAt, DateTime UpdatedAt);
