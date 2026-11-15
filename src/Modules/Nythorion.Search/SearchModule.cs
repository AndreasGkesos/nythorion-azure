using Microsoft.Extensions.DependencyInjection;

namespace Nythorion.Search;

public static class SearchModule
{
    public static IServiceCollection AddSearchModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SearchModule).Assembly));

        return services;
    }
}
