using MediatR;

namespace Nythorion.Learning.Features.GenerateQuiz;

public sealed record GenerateQuizCommand(Guid DocumentId, string UserId, int Count = 5) : IRequest<GenerateQuizResult>;

public sealed record GenerateQuizResult(Guid DocumentId, List<QuizQuestionDto> Questions);

public sealed record QuizQuestionDto(Guid Id, string Question, string[] Options, int CorrectOptionIndex);
