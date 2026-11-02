using Pgvector;

namespace Nythorion.Documents.Domain;

public class DocumentChunk
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public int ChunkIndex { get; private set; }
    public string Content { get; private set; } = default!;
    public Vector? Embedding { get; set; }
    public Document Document { get; private set; } = default!;

    private DocumentChunk() { }

    public static DocumentChunk Create(Guid documentId, int chunkIndex, string content)
    {
        return new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            ChunkIndex = chunkIndex,
            Content = content
        };
    }
}
