namespace Nythorion.Learning.Domain;

public class Note
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public string Content { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Note() { }

    public static Note Create(Guid documentId, string content) => new()
    {
        Id = Guid.NewGuid(),
        DocumentId = documentId,
        Content = content,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public void Update(string content)
    {
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }
}
