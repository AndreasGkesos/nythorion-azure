using FluentAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Nythorion.Learning.Generation;

namespace Nythorion.Learning.Tests;

public class FlashcardGeneratorTests
{
    private const string Question = "How does Retrieval Augmented Generation improve answers?";
    private const string Explanation = "It retrieves relevant chunks and gives them to the model as context.";

    [Fact]
    public async Task GenerateAsync_AnswerRepeatsQuestion_RemovesTheRepeat()
    {
        var generator = CreateGenerator($$"""{"question":"{{Question}}","answer":"{{Question}}\n\n{{Explanation}}"}""");

        var flashcard = await generator.GenerateAsync(Guid.NewGuid(), ["some text"], [], CancellationToken.None);

        flashcard!.Question.Should().Be(Question);
        flashcard.Answer.Should().Be(Explanation);
    }

    [Fact]
    public async Task GenerateAsync_AnswerDoesNotRepeatQuestion_KeepsAnswer()
    {
        var generator = CreateGenerator($$"""{"question":"{{Question}}","answer":"{{Explanation}}"}""");

        var flashcard = await generator.GenerateAsync(Guid.NewGuid(), ["some text"], [], CancellationToken.None);

        flashcard!.Answer.Should().Be(Explanation);
    }

    [Fact]
    public async Task GenerateAsync_AnswerIsOnlyTheQuestion_KeepsAnswerInsteadOfEmptyingIt()
    {
        var generator = CreateGenerator($$"""{"question":"{{Question}}","answer":"{{Question}}"}""");

        var flashcard = await generator.GenerateAsync(Guid.NewGuid(), ["some text"], [], CancellationToken.None);

        flashcard!.Answer.Should().Be(Question);
    }

    private static FlashcardGenerator CreateGenerator(string modelReply) =>
        new(new StubChatClient(modelReply), NullLogger<FlashcardGenerator>.Instance);

    private sealed class StubChatClient(string reply) : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, reply)));

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }
}
