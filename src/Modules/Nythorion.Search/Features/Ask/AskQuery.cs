using MediatR;

namespace Nythorion.Search.Features.Ask;

public sealed record AskQuery(string Question, Guid? DocumentId = null, string UserId = "") : IRequest<AskResult>;
