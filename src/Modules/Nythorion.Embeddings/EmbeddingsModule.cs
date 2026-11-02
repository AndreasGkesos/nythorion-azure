using Microsoft.Extensions.DependencyInjection;
using Nythorion.Embeddings.OllamaClient;

namespace Nythorion.Embeddings;

public static class EmbeddingsModule
{
    public static IServiceCollection AddEmbeddingsModule(this IServiceCollection services, string ollamaBaseUrl, string embedModel)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EmbeddingsModule).Assembly));

        services.AddHttpClient("ollama-embed", client =>
        {
            client.BaseAddress = new Uri(ollamaBaseUrl);
            client.Timeout = TimeSpan.FromMinutes(5); // embedding is fast; 5 min covers large batches on slow hardware
        });

        services.AddScoped<OllamaEmbeddingClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient("ollama-embed");
            return new OllamaEmbeddingClient(httpClient, embedModel);
        });

        return services;
    }
}
