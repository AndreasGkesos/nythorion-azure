using MediatR;
using Nythorion.Learning.Features.GenerateQuiz;

namespace Nythorion.Learning.Features.GenerateOneQuizQuestion;

public sealed record GenerateOneQuizQuestionCommand(Guid DocumentId, string UserId) : IRequest<QuizQuestionDto>;
