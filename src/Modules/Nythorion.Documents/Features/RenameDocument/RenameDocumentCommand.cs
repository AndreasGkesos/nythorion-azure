using MediatR;

namespace Nythorion.Documents.Features.RenameDocument;

public sealed record RenameDocumentCommand(Guid DocumentId, string UserId, string DisplayName) : IRequest;
