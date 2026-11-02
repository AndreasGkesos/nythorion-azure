using FluentAssertions;
using Nythorion.Documents.Domain;

namespace Nythorion.Documents.Tests;

public class DocumentTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var before = DateTime.UtcNow;

        var doc = Document.Create("user-1", "My Report", "report.pdf", "application/pdf", 1024);

        doc.Id.Should().NotBeEmpty();
        doc.UserId.Should().Be("user-1");
        doc.DisplayName.Should().Be("My Report");
        doc.FileName.Should().Be("report.pdf");
        doc.ContentType.Should().Be("application/pdf");
        doc.FileSizeBytes.Should().Be(1024);
        doc.Status.Should().Be(DocumentStatus.Uploading);
        doc.UploadedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Create_TwoDocuments_HaveDifferentIds()
    {
        var d1 = Document.Create("user-1", "Doc A", "a.pdf", "application/pdf", 100);
        var d2 = Document.Create("user-1", "Doc B", "b.pdf", "application/pdf", 200);

        d1.Id.Should().NotBe(d2.Id);
    }

    [Theory]
    [InlineData(DocumentStatus.Uploading)]
    [InlineData(DocumentStatus.Processing)]
    [InlineData(DocumentStatus.Ready)]
    [InlineData(DocumentStatus.Failed)]
    public void SetStatus_AllStatuses_AreAccepted(DocumentStatus status)
    {
        var doc = Document.Create("user-1", "Doc", "doc.pdf", "application/pdf", 100);

        doc.SetStatus(status);

        doc.Status.Should().Be(status);
    }

    [Fact]
    public void Rename_UpdatesDisplayName()
    {
        var doc = Document.Create("user-1", "Old Name", "doc.pdf", "application/pdf", 100);

        doc.Rename("New Name");

        doc.DisplayName.Should().Be("New Name");
    }

    [Fact]
    public void Rename_DoesNotAffectOtherProperties()
    {
        var doc = Document.Create("user-1", "Old Name", "doc.pdf", "application/pdf", 100);

        doc.Rename("New Name");

        doc.FileName.Should().Be("doc.pdf");
        doc.UserId.Should().Be("user-1");
        doc.Status.Should().Be(DocumentStatus.Uploading);
    }
}
