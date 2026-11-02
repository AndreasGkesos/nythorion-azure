using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Documents.Features.DeleteDocument;
using Nythorion.Embeddings.Data;

namespace Nythorion.Embeddings.Features.DeleteDocument;

internal sealed class DocumentDeletedHandler(IEmbeddingsDbContext db)
    : INotificationHandler<DocumentDeletedNotification>
{
    public async Task Handle(DocumentDeletedNotification notification, CancellationToken cancellationToken)
    {
        await db.DocumentChunks.Where(c => c.DocumentId == notification.DocumentId).ExecuteDeleteAsync(cancellationToken);
    }
}
