using Nythorion.Documents.Processing;

namespace Nythorion.Api.Processing;

public sealed class DocumentProcessingQueue(DocumentProcessingChannel channel) : IDocumentProcessingQueue
{
    public ValueTask EnqueueAsync(Guid documentId, CancellationToken cancellationToken = default)
        => channel.Writer.WriteAsync(documentId, cancellationToken);
}
