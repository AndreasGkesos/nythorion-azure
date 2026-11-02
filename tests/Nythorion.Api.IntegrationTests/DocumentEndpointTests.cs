using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Nythorion.Api.IntegrationTests.Infrastructure;

namespace Nythorion.Api.IntegrationTests;

public class DocumentEndpointTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    // ── Auth ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDocuments_WithoutToken_Returns401()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/documents");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDocuments_WithToken_Returns200()
    {
        var client = fixture.CreateClientForUser("user-auth-check");

        var response = await client.GetAsync("/documents");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Upload & List ─────────────────────────────────────────────────────

    [Fact]
    public async Task UploadDocument_Returns200_WithDocumentId()
    {
        var client = fixture.CreateClientForUser("user-upload");

        var response = await client.PostAsync("/documents/upload", BuildUploadForm("report.pdf", "My Report"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UploadResult>();
        body!.DocumentId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UploadedDocument_AppearsInList()
    {
        var client = fixture.CreateClientForUser("user-list");

        await client.PostAsync("/documents/upload", BuildUploadForm("list-test.pdf", "List Test Doc"));

        var docs = await client.GetFromJsonAsync<List<DocumentDto>>("/documents");
        docs.Should().Contain(d => d.DisplayName == "List Test Doc");
    }

    // ── User isolation ────────────────────────────────────────────────────

    [Fact]
    public async Task DocumentsUploadedByUserA_AreNotVisibleToUserB()
    {
        var userA = fixture.CreateClientForUser("user-isolation-a");
        var userB = fixture.CreateClientForUser("user-isolation-b");

        await userA.PostAsync("/documents/upload", BuildUploadForm("secret.pdf", "User A Secret Doc"));

        var userBDocs = await userB.GetFromJsonAsync<List<DocumentDto>>("/documents");
        userBDocs.Should().NotContain(d => d.DisplayName == "User A Secret Doc");
    }

    [Fact]
    public async Task DeleteDocument_ByWrongUser_Returns404()
    {
        var owner = fixture.CreateClientForUser("user-owner");
        var other = fixture.CreateClientForUser("user-other");

        var uploadResponse = await owner.PostAsync("/documents/upload", BuildUploadForm("owned.pdf", "Owned Doc"));
        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<UploadResult>();

        var deleteResponse = await other.DeleteAsync($"/documents/{uploaded!.DocumentId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Delete ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteDocument_ByOwner_Returns204_AndRemovedFromList()
    {
        var client = fixture.CreateClientForUser("user-delete");

        var uploadResponse = await client.PostAsync("/documents/upload", BuildUploadForm("todelete.pdf", "To Delete"));
        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<UploadResult>();

        var deleteResponse = await client.DeleteAsync($"/documents/{uploaded!.DocumentId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var docs = await client.GetFromJsonAsync<List<DocumentDto>>("/documents");
        docs.Should().NotContain(d => d.Id == uploaded.DocumentId);
    }

    // ── Rename ────────────────────────────────────────────────────────────

    [Fact]
    public async Task RenameDocument_UpdatesDisplayName()
    {
        var client = fixture.CreateClientForUser("user-rename");

        var uploadResponse = await client.PostAsync("/documents/upload", BuildUploadForm("rename-me.pdf", "Old Name"));
        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<UploadResult>();

        var renameResponse = await client.PatchAsJsonAsync(
            $"/documents/{uploaded!.DocumentId}/name",
            new { displayName = "New Name" });
        renameResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var docs = await client.GetFromJsonAsync<List<DocumentDto>>("/documents");
        docs.Should().Contain(d => d.Id == uploaded.DocumentId && d.DisplayName == "New Name");
    }

    [Fact]
    public async Task RenameDocument_ByWrongUser_Returns404()
    {
        var owner = fixture.CreateClientForUser("user-rename-owner");
        var other = fixture.CreateClientForUser("user-rename-other");

        var uploadResponse = await owner.PostAsync("/documents/upload", BuildUploadForm("rename-owned.pdf", "Owner Doc"));
        var uploaded = await uploadResponse.Content.ReadFromJsonAsync<UploadResult>();

        var renameResponse = await other.PatchAsJsonAsync(
            $"/documents/{uploaded!.DocumentId}/name",
            new { displayName = "Hijacked Name" });

        renameResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static MultipartFormDataContent BuildUploadForm(string fileName, string displayName)
    {
        var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(ApiFixture.MinimalPdfBytes) { Headers = { ContentType = new("application/pdf") } }, "file", fileName);
        form.Add(new StringContent(displayName), "displayName");
        return form;
    }

    private record UploadResult(Guid DocumentId, int ChunkCount);
    private record DocumentDto(Guid Id, string DisplayName, string FileName, string Status);
}
