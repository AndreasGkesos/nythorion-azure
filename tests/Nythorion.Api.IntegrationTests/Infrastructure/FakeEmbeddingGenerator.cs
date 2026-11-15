using Microsoft.Extensions.AI;

namespace Nythorion.Api.IntegrationTests.Infrastructure;

internal sealed class FakeEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private const int Dimensions = 1536;

    public static float[] Vector { get; } = BuildVector();

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(values.Select(_ => new Embedding<float>(Vector))));

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }

    private static float[] BuildVector()
    {
        var vector = new float[Dimensions];
        vector[0] = 1f;
        return vector;
    }
}
