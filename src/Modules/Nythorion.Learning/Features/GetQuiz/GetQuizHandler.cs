using MediatR;
using Microsoft.EntityFrameworkCore;
using Nythorion.Learning.Data;
using Nythorion.Learning.Features.GenerateQuiz;

namespace Nythorion.Learning.Features.GetQuiz;

internal sealed class GetQuizHandler(ILearningDbContext db)
    : IRequestHandler<GetQuizQuery, List<QuizQuestionDto>>
{
    public async Task<List<QuizQuestionDto>> Handle(GetQuizQuery request, CancellationToken cancellationToken)
    {
        return await db.QuizQuestions
            .Where(q => q.DocumentId == request.DocumentId)
            .Select(q => new QuizQuestionDto(q.Id, q.Question, q.Options, q.CorrectOptionIndex))
            .ToListAsync(cancellationToken);
    }
}
