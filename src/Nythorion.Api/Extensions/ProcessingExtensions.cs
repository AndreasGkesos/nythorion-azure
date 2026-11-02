using Nythorion.Api.Processing;
using Nythorion.Documents.Processing;

namespace Nythorion.Api.Extensions;

public static class ProcessingExtensions
{
    public static IServiceCollection AddProcessing(this IServiceCollection services)
    {
        services.AddSingleton<DocumentProcessingChannel>();
        services.AddScoped<IDocumentProcessingQueue, DocumentProcessingQueue>();
        services.AddHostedService<DocumentProcessingWorker>();
        return services;
    }
}
