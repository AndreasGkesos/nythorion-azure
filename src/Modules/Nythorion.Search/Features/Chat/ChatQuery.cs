using MediatR;

namespace Nythorion.Search.Features.Chat;

public sealed record ChatQuery(List<ChatTurn> Messages, Guid? DocumentId = null, string UserId = "") : IRequest<ChatResult>;

public sealed record ChatResult(string Answer, List<string> Sources);
