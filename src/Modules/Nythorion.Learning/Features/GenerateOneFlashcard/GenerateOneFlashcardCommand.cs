using MediatR;
using Nythorion.Learning.Features.GenerateFlashcards;

namespace Nythorion.Learning.Features.GenerateOneFlashcard;

public sealed record GenerateOneFlashcardCommand(Guid DocumentId, string UserId) : IRequest<FlashcardDto>;
