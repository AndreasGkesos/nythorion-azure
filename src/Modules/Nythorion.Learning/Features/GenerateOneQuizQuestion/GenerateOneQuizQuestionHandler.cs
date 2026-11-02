using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Learning.Data;
using Nythorion.Learning.Domain;
using Nythorion.Learning.Features.GenerateQuiz;

namespace Nythorion.Learning.Features.GenerateOneQuizQuestion;

internal sealed class GenerateOneQuizQuestionHandler(ILearningDbContext db)
    : IRequestHandler<GenerateOneQuizQuestionCommand, QuizQuestionDto>
{
    public async Task<QuizQuestionDto> Handle(GenerateOneQuizQuestionCommand request, CancellationToken cancellationToken)
    {
        if (!await db.Documents.AnyAsync(d => d.Id == request.DocumentId && d.UserId == request.UserId, cancellationToken))
            throw new KeyNotFoundException($"Document {request.DocumentId} not found.");

        var flashcards = await db.Flashcards
            .Where(f => f.DocumentId == request.DocumentId)
            .ToListAsync(cancellationToken);

        var existingQuestions = await db.QuizQuestions
            .Where(q => q.DocumentId == request.DocumentId)
            .Select(q => q.Question)
            .ToListAsync(cancellationToken);

        if (flashcards.Count < 2)
            throw new InvalidOperationException($"Need at least 2 flashcards to generate a quiz question, found {flashcards.Count}.");

        var unused = flashcards.Where(f => !existingQuestions.Contains(f.Question)).ToList();
        var card = (unused.Count > 0 ? unused : flashcards).OrderBy(_ => Random.Shared.Next()).First();

        var wrongAnswers = flashcards
            .Where(f => f.Id != card.Id)
            .OrderBy(_ => Random.Shared.Next())
            .Select(f => f.Answer)
            .Distinct()
            .Take(3)
            .ToList();

        if (wrongAnswers.Count == 0)
            throw new InvalidOperationException("Not enough distinct flashcards to generate wrong options.");

        var correctIndex = Random.Shared.Next(0, wrongAnswers.Count + 1);
        var options = new List<string>(wrongAnswers);
        options.Insert(correctIndex, card.Answer);

        var question = QuizQuestion.Create(request.DocumentId, card.Question, options.ToArray(), correctIndex);
        db.QuizQuestions.Add(question);
        await db.SaveChangesAsync(cancellationToken);

        return new QuizQuestionDto(question.Id, question.Question, question.Options, question.CorrectOptionIndex);
    }
}
