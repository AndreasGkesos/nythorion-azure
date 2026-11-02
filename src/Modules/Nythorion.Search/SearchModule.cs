using Microsoft.Extensions.DependencyInjection;
using Nythorion.Search.OllamaClient;

namespace Nythorion.Search;

public static class SearchModule
{
    public static IServiceCollection AddSearchModule(this IServiceCollection services, string ollamaBaseUrl, string llmModel, float temperature = 0.1f, int numPredict = 4096)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SearchModule).Assembly));

        services.AddHttpClient("ollama-search-llm", client =>
        {
            client.BaseAddress = new Uri(ollamaBaseUrl);
            client.Timeout = TimeSpan.FromMinutes(10); // LLM generation (RAG answers) can take several minutes on local hardware
        });

        services.AddScoped<OllamaLlmClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient("ollama-search-llm");
            return new OllamaLlmClient(httpClient, llmModel, temperature, numPredict);
        });

        return services;
    }
}
