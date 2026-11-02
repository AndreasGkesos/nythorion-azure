using MediatR;

namespace Nythorion.Learning.Features.GenerateFlashcards;

public sealed record GenerateFlashcardsCommand(Guid DocumentId, string UserId, int Count = 10) : IRequest<GenerateFlashcardsResult>;

public sealed record GenerateFlashcardsResult(Guid DocumentId, List<FlashcardDto> Flashcards);

public sealed record FlashcardDto(Guid Id, string Question, string Answer);
