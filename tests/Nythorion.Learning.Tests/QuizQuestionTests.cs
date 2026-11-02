using FluentAssertions;
using Nythorion.Learning.Domain;

namespace Nythorion.Learning.Tests;

public class QuizQuestionTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var documentId = Guid.NewGuid();
        var options = new[] { "Option A", "Option B", "Option C", "Option D" };
        var before = DateTime.UtcNow;

        var question = QuizQuestion.Create(documentId, "What is dependency injection?", options, correctOptionIndex: 2);

        question.Id.Should().NotBeEmpty();
        question.DocumentId.Should().Be(documentId);
        question.Question.Should().Be("What is dependency injection?");
        question.Options.Should().BeEquivalentTo(options);
        question.CorrectOptionIndex.Should().Be(2);
        question.CreatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Create_TwoQuestions_HaveDifferentIds()
    {
        var documentId = Guid.NewGuid();
        var options = new[] { "A", "B", "C", "D" };

        var q1 = QuizQuestion.Create(documentId, "Question 1?", options, 0);
        var q2 = QuizQuestion.Create(documentId, "Question 2?", options, 1);

        q1.Id.Should().NotBe(q2.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Create_AllCorrectOptionIndexes_AreStored(int correctIndex)
    {
        var options = new[] { "A", "B", "C", "D" };

        var question = QuizQuestion.Create(Guid.NewGuid(), "Q?", options, correctIndex);

        question.CorrectOptionIndex.Should().Be(correctIndex);
    }
}
