using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Learning.Data;

namespace Nythorion.Learning.Features.DeleteQuizQuestion;

internal sealed class DeleteQuizQuestionHandler(ILearningDbContext db)
    : IRequestHandler<DeleteQuizQuestionCommand>
{
    public async Task Handle(DeleteQuizQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await db.QuizQuestions
            .FirstOrDefaultAsync(q => q.Id == request.QuizQuestionId, cancellationToken)
            ?? throw new InvalidOperationException($"Quiz question {request.QuizQuestionId} not found.");

        db.QuizQuestions.Remove(question);
        await db.SaveChangesAsync(cancellationToken);
    }
}
