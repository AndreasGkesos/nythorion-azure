using Microsoft.Extensions.DependencyInjection;
using Nythorion.Learning.Generation;

namespace Nythorion.Learning;

public static class LearningModule
{
    public static IServiceCollection AddLearningModule(this IServiceCollection services)
    {
        services.AddScoped<FlashcardGenerator>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LearningModule).Assembly));

        return services;
    }
}
