using Microsoft.Extensions.DependencyInjection;
using Nythorion.Documents.Parsing;

namespace Nythorion.Documents;

public static class DocumentsModule
{
    public static IServiceCollection AddDocumentsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DocumentsModule).Assembly));

        services.AddScoped<IDocumentParser, PdfParser>();
        services.AddScoped<IDocumentParser, DocxParser>();

        return services;
    }
}
