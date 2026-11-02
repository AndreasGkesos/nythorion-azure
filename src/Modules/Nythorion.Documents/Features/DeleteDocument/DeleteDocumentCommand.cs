using MediatR;

namespace Nythorion.Documents.Features.DeleteDocument;

public sealed record DeleteDocumentCommand(Guid DocumentId, string UserId) : IRequest;
