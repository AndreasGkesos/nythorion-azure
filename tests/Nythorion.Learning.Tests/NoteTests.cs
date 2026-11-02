using FluentAssertions;
using Nythorion.Learning.Domain;

namespace Nythorion.Learning.Tests;

public class NoteTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var documentId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var note = Note.Create(documentId, "My note content");

        note.Id.Should().NotBeEmpty();
        note.DocumentId.Should().Be(documentId);
        note.Content.Should().Be("My note content");
        note.CreatedAt.Should().BeOnOrAfter(before);
        note.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Create_TwoNotes_HaveDifferentIds()
    {
        var documentId = Guid.NewGuid();

        var note1 = Note.Create(documentId, "First");
        var note2 = Note.Create(documentId, "Second");

        note1.Id.Should().NotBe(note2.Id);
    }

    [Fact]
    public void Update_ChangesContentAndUpdatedAt()
    {
        var note = Note.Create(Guid.NewGuid(), "Original");
        var createdAt = note.CreatedAt;

        Thread.Sleep(1);
        note.Update("Updated content");

        note.Content.Should().Be("Updated content");
        note.CreatedAt.Should().Be(createdAt);
        note.UpdatedAt.Should().BeAfter(createdAt);
    }
}
