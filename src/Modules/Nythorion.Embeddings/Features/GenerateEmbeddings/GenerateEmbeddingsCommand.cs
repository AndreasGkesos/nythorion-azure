using MediatR;

namespace Nythorion.Embeddings.Features.GenerateEmbeddings;

public record GenerateEmbeddingsCommand(Guid DocumentId) : IRequest<GenerateEmbeddingsResult>;

public record GenerateEmbeddingsResult(Guid DocumentId, int EmbeddingsGenerated);
