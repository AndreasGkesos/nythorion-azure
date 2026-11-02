namespace Nythorion.Learning.Domain;

public class Flashcard
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public string Question { get; private set; } = default!;
    public string Answer { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private Flashcard() { }

    public static Flashcard Create(Guid documentId, string question, string answer) => new()
    {
        Id = Guid.NewGuid(),
        DocumentId = documentId,
        Question = question,
        Answer = answer,
        CreatedAt = DateTime.UtcNow
    };
}
