using MediatR;

namespace Nythorion.Documents.Features.DeleteDocument;

public sealed record DocumentDeletedNotification(Guid DocumentId) : INotification;
