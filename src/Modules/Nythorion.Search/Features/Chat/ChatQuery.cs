using MediatR;
using Nythorion.Search.OllamaClient;

namespace Nythorion.Search.Features.Chat;

public sealed record ChatQuery(List<ChatMessage> Messages, Guid? DocumentId = null) : IRequest<ChatResult>;

public sealed record ChatResult(string Answer, List<string> Sources);
