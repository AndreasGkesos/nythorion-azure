namespace Nythorion.Learning.Domain;

public class QuizQuestion
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public string Question { get; private set; } = default!;
    public string[] Options { get; private set; } = default!;
    public int CorrectOptionIndex { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private QuizQuestion() { }

    public static QuizQuestion Create(Guid documentId, string question, string[] options, int correctOptionIndex) => new()
    {
        Id = Guid.NewGuid(),
        DocumentId = documentId,
        Question = question,
        Options = options,
        CorrectOptionIndex = correctOptionIndex,
        CreatedAt = DateTime.UtcNow
    };
}
