namespace Nythorion.Documents.Processing;

public interface IDocumentProcessingQueue
{
    ValueTask EnqueueAsync(Guid documentId, CancellationToken cancellationToken = default);
}
