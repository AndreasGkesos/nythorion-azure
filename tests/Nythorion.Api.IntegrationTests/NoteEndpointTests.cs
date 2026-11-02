using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Nythorion.Api.IntegrationTests.Infrastructure;

namespace Nythorion.Api.IntegrationTests;

public class NoteEndpointTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task AddNote_Returns200_WithNoteId()
    {
        var client = fixture.CreateClientForUser("user-note-add");
        var docId = await DocumentHelpers.UploadDocumentAsync(client);

        var response = await client.PostAsJsonAsync($"/documents/{docId}/notes", new { content = "My first note" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var note = await response.Content.ReadFromJsonAsync<NoteDto>();
        note!.Id.Should().NotBeEmpty();
        note.Content.Should().Be("My first note");
    }

    [Fact]
    public async Task GetNotes_ReturnsAddedNotes()
    {
        var client = fixture.CreateClientForUser("user-note-get");
        var docId = await DocumentHelpers.UploadDocumentAsync(client);

        await client.PostAsJsonAsync($"/documents/{docId}/notes", new { content = "Note one" });
        await client.PostAsJsonAsync($"/documents/{docId}/notes", new { content = "Note two" });

        var notes = await client.GetFromJsonAsync<List<NoteDto>>($"/documents/{docId}/notes");
        notes.Should().HaveCount(2);
        notes.Should().Contain(n => n.Content == "Note one");
        notes.Should().Contain(n => n.Content == "Note two");
    }

    [Fact]
    public async Task GetNotes_NoNotes_ReturnsEmptyList()
    {
        var client = fixture.CreateClientForUser("user-note-empty");
        var docId = await DocumentHelpers.UploadDocumentAsync(client);

        var notes = await client.GetFromJsonAsync<List<NoteDto>>($"/documents/{docId}/notes");

        notes.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteNote_Returns204_AndRemovedFromList()
    {
        var client = fixture.CreateClientForUser("user-note-delete");
        var docId = await DocumentHelpers.UploadDocumentAsync(client);

        var addResponse = await client.PostAsJsonAsync($"/documents/{docId}/notes", new { content = "To delete" });
        var note = await addResponse.Content.ReadFromJsonAsync<NoteDto>();

        var deleteResponse = await client.DeleteAsync($"/documents/{docId}/notes/{note!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var notes = await client.GetFromJsonAsync<List<NoteDto>>($"/documents/{docId}/notes");
        notes.Should().NotContain(n => n.Id == note.Id);
    }

    [Fact]
    public async Task GetNotes_WithoutToken_Returns401()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync($"/documents/{Guid.NewGuid()}/notes");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private record NoteDto(Guid Id, string Content);
}
