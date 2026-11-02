namespace Nythorion.Learning.Domain;

public class Summary
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public string Content { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private Summary() { }

    public static Summary Create(Guid documentId, string content) => new()
    {
        Id = Guid.NewGuid(),
        DocumentId = documentId,
        Content = content,
        CreatedAt = DateTime.UtcNow
    };
}
