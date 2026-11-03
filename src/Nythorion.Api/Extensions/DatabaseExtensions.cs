using Microsoft.EntityFrameworkCore;
using Nythorion.Api.Data;
using Nythorion.Documents.Data;
using Nythorion.Embeddings.Data;
using Nythorion.Learning.Data;
using Nythorion.Search.Data;

namespace Nythorion.Api.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NythorionDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Default"),
                o => o.UseVector()));

        services.AddScoped<IDocumentsDbContext>(sp => sp.GetRequiredService<NythorionDbContext>());
        services.AddScoped<IEmbeddingsDbContext>(sp => sp.GetRequiredService<NythorionDbContext>());
        services.AddScoped<ISearchDbContext>(sp => sp.GetRequiredService<NythorionDbContext>());
        services.AddScoped<ILearningDbContext>(sp => sp.GetRequiredService<NythorionDbContext>());

        // Registered before the processing worker so migrations finish first (hosted services start in order)
        services.AddHostedService<DatabaseMigrator>();

        return services;
    }
}
