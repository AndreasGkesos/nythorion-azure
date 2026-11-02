using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Nythorion.Api.IntegrationTests.Infrastructure;

namespace Nythorion.Api.IntegrationTests;

public class QuizEndpointTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    // ── Auth ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetQuiz_WithoutToken_Returns401()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync($"/documents/{Guid.NewGuid()}/quiz");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Get ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetQuiz_NoQuestions_ReturnsEmptyList()
    {
        var client = fixture.CreateClientForUser("user-quiz-empty");
        var docId = await DocumentHelpers.UploadDocumentAsync(client);

        var questions = await client.GetFromJsonAsync<List<QuizQuestionDto>>($"/documents/{docId}/quiz");

        questions.Should().BeEmpty();
    }

    // ── Batch generate — requires flashcards, returns 400 without them ────

    [Fact]
    public async Task GenerateQuizBatch_WithoutFlashcards_Returns400()
    {
        var client = fixture.CreateClientForUser("user-quiz-batch-guard");
        var docId = await DocumentHelpers.UploadDocumentAsync(client);

        var response = await client.PostAsJsonAsync($"/documents/{docId}/quiz/generate/batch", new { count = 5 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateQuizBatch_WithoutToken_Returns401()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync($"/documents/{Guid.NewGuid()}/quiz/generate/batch", new { count = 5 });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Delete ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteQuizQuestion_WithoutToken_Returns401()
    {
        var client = fixture.CreateClient();

        var response = await client.DeleteAsync($"/documents/{Guid.NewGuid()}/quiz/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private record QuizQuestionDto(Guid Id, string Question, string[] Options, int CorrectOptionIndex);
}
