using System.Net.Http.Json;

namespace Nythorion.Embeddings.OllamaClient;

public class OllamaEmbeddingClient(HttpClient httpClient, string model)
{
    public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/embed", new
        {
            model,
            input = text
        }, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>(cancellationToken: cancellationToken);
        return result!.Embeddings[0];
    }

    private sealed record OllamaEmbedResponse(float[][] Embeddings);
}
