using FluentAssertions;
using Nythorion.Learning.Domain;

namespace Nythorion.Learning.Tests;

public class SummaryTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var documentId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var summary = Summary.Create(documentId, "This is a summary of the document.");

        summary.Id.Should().NotBeEmpty();
        summary.DocumentId.Should().Be(documentId);
        summary.Content.Should().Be("This is a summary of the document.");
        summary.CreatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Create_TwoSummaries_HaveDifferentIds()
    {
        var documentId = Guid.NewGuid();

        var s1 = Summary.Create(documentId, "First summary");
        var s2 = Summary.Create(documentId, "Second summary");

        s1.Id.Should().NotBe(s2.Id);
    }
}
