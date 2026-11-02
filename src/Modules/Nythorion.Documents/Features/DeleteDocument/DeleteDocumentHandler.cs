using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Documents.Data;

namespace Nythorion.Documents.Features.DeleteDocument;

internal sealed class DeleteDocumentHandler(IDocumentsDbContext db, IPublisher publisher)
    : IRequestHandler<DeleteDocumentCommand>
{
    public async Task Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await db.Documents
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId && d.UserId == request.UserId, cancellationToken);

        if (document is null) throw new KeyNotFoundException($"Document {request.DocumentId} not found.");

        db.Documents.Remove(document);
        await db.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new DocumentDeletedNotification(request.DocumentId), cancellationToken);
    }
}
