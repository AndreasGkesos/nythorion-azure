using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace Nythorion.Api.Extensions;

public static class AiExtensions
{
    private const string SectionName = "AzureOpenAI";

    public static IServiceCollection AddAzureOpenAi(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);

        // Created on first use so the app and its tests start without an endpoint configured
        services.AddSingleton(_ => new AzureOpenAIClient(
            new Uri(section["Endpoint"]!),
            new DefaultAzureCredential()));

        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
            sp.GetRequiredService<AzureOpenAIClient>()
                .GetEmbeddingClient(section["EmbeddingDeployment"]!)
                .AsIEmbeddingGenerator());

        return services;
    }
}
