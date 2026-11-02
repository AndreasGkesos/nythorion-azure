namespace Nythorion.Search.Features.SemanticSearch;

public sealed record SemanticSearchResult(
    Guid DocumentId,
    string FileName,
    Guid ChunkId,
    int ChunkIndex,
    string Content,
    double Score);
