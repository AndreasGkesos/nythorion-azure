using MediatR;

namespace Nythorion.Search.Features.SemanticSearch;

public sealed record SemanticSearchQuery(string Text, Guid? DocumentId = null, int TopK = 5) : IRequest<List<SemanticSearchResult>>;
