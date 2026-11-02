using MediatR;

namespace Nythorion.Learning.Features.DeleteQuizQuestion;

public sealed record DeleteQuizQuestionCommand(Guid QuizQuestionId) : IRequest;
