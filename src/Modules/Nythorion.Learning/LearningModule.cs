using Microsoft.Extensions.DependencyInjection;
using Nythorion.Learning.OllamaClient;

namespace Nythorion.Learning;

public static class LearningModule
{
    public static IServiceCollection AddLearningModule(this IServiceCollection services, string ollamaBaseUrl, string llmModel, float temperature = 0.1f, int numPredict = 4096)
    {
        services.AddHttpClient("ollama-llm", client =>
        {
            client.BaseAddress = new Uri(ollamaBaseUrl);
            client.Timeout = TimeSpan.FromMinutes(10); // LLM generation (summaries, flashcards) can take several minutes on local hardware
        });

        services.AddScoped<OllamaLlmClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient("ollama-llm");
            return new OllamaLlmClient(httpClient, llmModel, temperature, numPredict);
        });

        services.AddScoped<FlashcardGenerator>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LearningModule).Assembly));

        return services;
    }
}
