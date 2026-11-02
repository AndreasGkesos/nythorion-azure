using MediatR;
using Nythorion.Search.Features.Ask;
using Nythorion.Search.Features.Chat;
using Nythorion.Search.Features.SemanticSearch;

namespace Nythorion.Api.Extensions;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/search", async (SemanticSearchQuery query, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var results = await mediator.Send(query, cancellationToken);
            return Results.Ok(results);
        })
        .RequireAuthorization();

        app.MapPost("/search/ask", async (AskQuery query, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization();

        app.MapPost("/search/chat", async (ChatQuery query, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization();

        return app;
    }
}
