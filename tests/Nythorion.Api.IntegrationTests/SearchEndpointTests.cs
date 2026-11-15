using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Nythorion.Api.IntegrationTests.Infrastructure;

namespace Nythorion.Api.IntegrationTests;

public class SearchEndpointTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    [Theory]
    [InlineData("/search")]
    [InlineData("/search/ask")]
    [InlineData("/search/chat")]
    public async Task Search_WithoutToken_Returns401(string path)
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync(path, new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Search_ReturnsOnlyTheCallersChunks()
    {
        var ownDocument = await fixture.SeedEmbeddedDocumentAsync("search-owner", "owner content");
        var otherDocument = await fixture.SeedEmbeddedDocumentAsync("search-other", "other content");
        var client = fixture.CreateClientForUser("search-owner");

        var response = await client.PostAsJsonAsync("/search", new { text = "anything", topK = 50 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var hits = await response.Content.ReadFromJsonAsync<List<SearchHit>>();
        hits!.Select(h => h.DocumentId).Should().Contain(ownDocument).And.NotContain(otherDocument);
    }

    [Fact]
    public async Task Search_ScopedToAnotherUsersDocument_ReturnsNothing()
    {
        await fixture.SeedEmbeddedDocumentAsync("scope-owner", "owner content");
        var otherDocument = await fixture.SeedEmbeddedDocumentAsync("scope-other", "other content");
        var client = fixture.CreateClientForUser("scope-owner");

        var response = await client.PostAsJsonAsync("/search", new { text = "anything", documentId = otherDocument });

        var hits = await response.Content.ReadFromJsonAsync<List<SearchHit>>();
        hits.Should().BeEmpty();
    }

    [Fact]
    public async Task Ask_UsesOnlyTheCallersDocumentsAsSources()
    {
        var ownDocument = await fixture.SeedEmbeddedDocumentAsync("ask-owner", "owner content");
        var otherDocument = await fixture.SeedEmbeddedDocumentAsync("ask-other", "other content");
        var client = fixture.CreateClientForUser("ask-owner");

        var response = await client.PostAsJsonAsync("/search/ask", new { question = "anything" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AnswerResult>();
        result!.Answer.Should().Be(FakeChatClient.Answer);
        result.Sources.Should().Contain(ownDocument.ToString()).And.NotContain(otherDocument.ToString());
    }

    [Fact]
    public async Task Chat_UsesOnlyTheCallersDocumentsAsSources()
    {
        var ownDocument = await fixture.SeedEmbeddedDocumentAsync("chat-owner", "owner content");
        var otherDocument = await fixture.SeedEmbeddedDocumentAsync("chat-other", "other content");
        var client = fixture.CreateClientForUser("chat-owner");

        var response = await client.PostAsJsonAsync("/search/chat", new
        {
            messages = new[] { new { role = "user", content = "anything" } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AnswerResult>();
        result!.Answer.Should().Be(FakeChatClient.Answer);
        result.Sources.Should().Contain(ownDocument.ToString()).And.NotContain(otherDocument.ToString());
    }

    private sealed record SearchHit(Guid DocumentId);

    private sealed record AnswerResult(string Answer, List<string> Sources);
}
