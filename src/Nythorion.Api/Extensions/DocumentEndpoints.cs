using System.Security.Claims;
using MediatR;
using Nythorion.Documents.Features.DeleteDocument;
using Nythorion.Documents.Features.GetDocuments;
using Nythorion.Documents.Features.RenameDocument;
using Nythorion.Documents.Features.UploadDocument;

namespace Nythorion.Api.Extensions;

public static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/documents", async (ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetDocumentsQuery(user.GetUserId()), cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization();

        app.MapPost("/documents/upload", async (IFormFile file, IFormCollection form, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var displayName = form["displayName"].FirstOrDefault()?.Trim();
            if (string.IsNullOrEmpty(displayName))
                displayName = Path.GetFileNameWithoutExtension(file.FileName);
            if (displayName.Length > 255)
                return Results.BadRequest("Display name cannot exceed 255 characters.");

            var command = new UploadDocumentCommand(
                user.GetUserId(),
                displayName,
                file.OpenReadStream(),
                file.FileName,
                file.ContentType,
                file.Length);

            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .DisableAntiforgery();

        app.MapDelete("/documents/{id:guid}", async (Guid id, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
        {
            try
            {
                await mediator.Send(new DeleteDocumentCommand(id, user.GetUserId()), cancellationToken);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .RequireAuthorization();

        app.MapPatch("/documents/{id:guid}/name", async (Guid id, RenameRequest body, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(body.DisplayName))
                return Results.BadRequest("Display name cannot be empty.");
            if (body.DisplayName.Trim().Length > 255)
                return Results.BadRequest("Display name cannot exceed 255 characters.");

            try
            {
                await mediator.Send(new RenameDocumentCommand(id, user.GetUserId(), body.DisplayName.Trim()), cancellationToken);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .RequireAuthorization();

        return app;
    }

}

public sealed record RenameRequest(string DisplayName);
