using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Nythorion.Api.IntegrationTests.Infrastructure;

namespace Nythorion.Api.IntegrationTests;

public class FlashcardEndpointTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    // ── Auth ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetFlashcards_WithoutToken_Returns401()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync($"/documents/{Guid.NewGuid()}/flashcards");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Get ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetFlashcards_NoFlashcards_ReturnsEmptyList()
    {
        var client = fixture.CreateClientForUser("user-flashcard-empty");
        var docId = await DocumentHelpers.UploadDocumentAsync(client);

        var flashcards = await client.GetFromJsonAsync<List<FlashcardDto>>($"/documents/{docId}/flashcards");

        flashcards.Should().BeEmpty();
    }

    // ── Batch generate — no Ollama, just test the 400 guard ───────────────

    [Fact]
    public async Task GenerateFlashcardsBatch_WithoutToken_Returns401()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync($"/documents/{Guid.NewGuid()}/flashcards/generate/batch", new { count = 5 });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Delete ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteFlashcard_WithoutToken_Returns401()
    {
        var client = fixture.CreateClient();

        var response = await client.DeleteAsync($"/documents/{Guid.NewGuid()}/flashcards/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private record FlashcardDto(Guid Id, string Question, string Answer);
}
