using Microsoft.Extensions.DependencyInjection;

namespace Nythorion.Embeddings;

public static class EmbeddingsModule
{
    public static IServiceCollection AddEmbeddingsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EmbeddingsModule).Assembly));

        return services;
    }
}
