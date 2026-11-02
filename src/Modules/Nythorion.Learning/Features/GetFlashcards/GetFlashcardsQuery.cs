using MediatR;
using Nythorion.Learning.Features.GenerateFlashcards;

namespace Nythorion.Learning.Features.GetFlashcards;

public sealed record GetFlashcardsQuery(Guid DocumentId) : IRequest<List<FlashcardDto>>;
