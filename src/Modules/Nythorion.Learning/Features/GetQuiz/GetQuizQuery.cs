using MediatR;
using Nythorion.Learning.Features.GenerateQuiz;

namespace Nythorion.Learning.Features.GetQuiz;

public sealed record GetQuizQuery(Guid DocumentId) : IRequest<List<QuizQuestionDto>>;
