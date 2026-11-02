using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Learning.Data;
using Nythorion.Learning.Domain;

namespace Nythorion.Learning.Features.GenerateQuiz;

internal sealed class GenerateQuizHandler(ILearningDbContext db)
    : IRequestHandler<GenerateQuizCommand, GenerateQuizResult>
{
    public async Task<GenerateQuizResult> Handle(GenerateQuizCommand request, CancellationToken cancellationToken)
    {
        if (!await db.Documents.AnyAsync(d => d.Id == request.DocumentId && d.UserId == request.UserId, cancellationToken))
            throw new KeyNotFoundException($"Document {request.DocumentId} not found.");

        var flashcards = await db.Flashcards
            .Where(f => f.DocumentId == request.DocumentId)
            .ToListAsync(cancellationToken);

        if (flashcards.Count < 2)
            throw new InvalidOperationException($"Need at least 2 flashcards to generate a quiz, found {flashcards.Count}.");

        var candidates = flashcards.OrderBy(_ => Random.Shared.Next()).Take(request.Count).ToList();

        var questions = candidates.Select(card =>
        {
            var wrongAnswers = flashcards
                .Where(f => f.Id != card.Id)
                .OrderBy(_ => Random.Shared.Next())
                .Select(f => f.Answer)
                .Distinct()
                .Take(3)
                .ToList();

            // Need at least 1 wrong answer to make a valid question
            if (wrongAnswers.Count == 0)
                return null;

            var correctIndex = Random.Shared.Next(0, wrongAnswers.Count + 1);
            var options = new List<string>(wrongAnswers);
            options.Insert(correctIndex, card.Answer);

            return QuizQuestion.Create(request.DocumentId, card.Question, options.ToArray(), correctIndex);
        })
        .Where(q => q is not null)
        .Cast<QuizQuestion>()
        .ToList();

        db.QuizQuestions.AddRange(questions);
        await db.SaveChangesAsync(cancellationToken);

        return new GenerateQuizResult(
            request.DocumentId,
            questions.Select(q => new QuizQuestionDto(q.Id, q.Question, q.Options, q.CorrectOptionIndex)).ToList());
    }
}
