using MediatR;

namespace Nythorion.Learning.Features.DeleteFlashcard;

public sealed record DeleteFlashcardCommand(Guid FlashcardId) : IRequest;
