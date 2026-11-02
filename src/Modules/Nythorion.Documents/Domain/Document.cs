namespace Nythorion.Documents.Domain;

public class Document
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long FileSizeBytes { get; private set; }
    public DocumentStatus Status { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public ICollection<DocumentChunk> Chunks { get; private set; } = [];

    private Document() { }

    public static Document Create(string userId, string displayName, string fileName, string contentType, long fileSizeBytes)
    {
        return new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DisplayName = displayName,
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            Status = DocumentStatus.Uploading,
            UploadedAt = DateTime.UtcNow
        };
    }

    public void SetStatus(DocumentStatus status) => Status = status;

    public void Rename(string displayName) => DisplayName = displayName;
}
