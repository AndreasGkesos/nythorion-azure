using FluentAssertions;
using Nythorion.Learning.Domain;

namespace Nythorion.Learning.Tests;

public class FlashcardTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var documentId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var flashcard = Flashcard.Create(documentId, "What is SOLID?", "A set of five design principles.");

        flashcard.Id.Should().NotBeEmpty();
        flashcard.DocumentId.Should().Be(documentId);
        flashcard.Question.Should().Be("What is SOLID?");
        flashcard.Answer.Should().Be("A set of five design principles.");
        flashcard.CreatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Create_TwoFlashcards_HaveDifferentIds()
    {
        var documentId = Guid.NewGuid();

        var f1 = Flashcard.Create(documentId, "Q1", "A1");
        var f2 = Flashcard.Create(documentId, "Q2", "A2");

        f1.Id.Should().NotBe(f2.Id);
    }
}
