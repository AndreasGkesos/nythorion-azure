using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace Nythorion.Api.Extensions;

public static class AiExtensions
{
    private const string SectionName = "AzureOpenAI";
    private const string TemperatureKey = "Temperature";
    private const string MaxOutputTokensKey = "MaxOutputTokens";
    private const float DefaultTemperature = 0.1f;
    private const int DefaultMaxOutputTokens = 4096;

    public static IServiceCollection AddAzureOpenAi(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);

        // Created on first use so the app and its tests start without an endpoint configured
        services.AddSingleton(_ => new AzureOpenAIClient(
            new Uri(section["Endpoint"]!),
            new DefaultAzureCredential()));

        services.AddSingleton<IChatClient>(sp =>
            new ChatClientBuilder(
                    sp.GetRequiredService<AzureOpenAIClient>()
                        .GetChatClient(section["ChatDeployment"]!)
                        .AsIChatClient())
                .ConfigureOptions(options =>
                {
                    options.Temperature = section.GetValue(TemperatureKey, DefaultTemperature);
                    options.MaxOutputTokens = section.GetValue(MaxOutputTokensKey, DefaultMaxOutputTokens);
                })
                .Build());

        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
            sp.GetRequiredService<AzureOpenAIClient>()
                .GetEmbeddingClient(section["EmbeddingDeployment"]!)
                .AsIEmbeddingGenerator());

        return services;
    }
}
