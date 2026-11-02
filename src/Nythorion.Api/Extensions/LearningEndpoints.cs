using System.Security.Claims;
using MediatR;
using Nythorion.Learning.Features.AddNote;
using Nythorion.Learning.Features.DeleteFlashcard;
using Nythorion.Learning.Features.DeleteNote;
using Nythorion.Learning.Features.DeleteQuizQuestion;
using Nythorion.Learning.Features.GenerateFlashcards;
using Nythorion.Learning.Features.GenerateOneFlashcard;
using Nythorion.Learning.Features.GenerateOneQuizQuestion;
using Nythorion.Learning.Features.GenerateQuiz;
using Nythorion.Learning.Features.GenerateSummary;
using Nythorion.Learning.Features.GetFlashcards;
using Nythorion.Learning.Features.GetNotes;
using Nythorion.Learning.Features.GetQuiz;
using Nythorion.Learning.Features.GetSummary;

namespace Nythorion.Api.Extensions;

public static class LearningEndpoints
{
    public static IEndpointRouteBuilder MapLearningEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/documents/{documentId}/summary", async (Guid documentId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetSummaryQuery(documentId), cancellationToken);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .RequireAuthorization();

        app.MapPost("/documents/{documentId}/summary/generate", async (Guid documentId, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await mediator.Send(new GenerateSummaryCommand(documentId, user.GetUserId()), cancellationToken);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        })
        .RequireAuthorization();

        app.MapGet("/documents/{documentId}/flashcards", async (Guid documentId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetFlashcardsQuery(documentId), cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization();

        app.MapPost("/documents/{documentId}/flashcards/generate", async (Guid documentId, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await mediator.Send(new GenerateOneFlashcardCommand(documentId, user.GetUserId()), cancellationToken);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(ex.Message); }
        })
        .RequireAuthorization();

        app.MapPost("/documents/{documentId}/flashcards/generate/batch", async (Guid documentId, GenerateFlashcardsRequest? body, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
        {
            try
            {
                var count = body?.Count ?? 5;
                var result = await mediator.Send(new GenerateFlashcardsCommand(documentId, user.GetUserId(), count), cancellationToken);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        })
        .RequireAuthorization();

        app.MapDelete("/documents/{documentId}/flashcards/{flashcardId}", async (Guid documentId, Guid flashcardId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteFlashcardCommand(flashcardId), cancellationToken);
            return Results.NoContent();
        })
        .RequireAuthorization();

        app.MapGet("/documents/{documentId}/quiz", async (Guid documentId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetQuizQuery(documentId), cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization();

        app.MapPost("/documents/{documentId}/quiz/generate", async (Guid documentId, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await mediator.Send(new GenerateOneQuizQuestionCommand(documentId, user.GetUserId()), cancellationToken);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(ex.Message); }
        })
        .RequireAuthorization();

        app.MapPost("/documents/{documentId}/quiz/generate/batch", async (Guid documentId, GenerateQuizRequest? body, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
        {
            try
            {
                var count = body?.Count ?? 5;
                var result = await mediator.Send(new GenerateQuizCommand(documentId, user.GetUserId(), count), cancellationToken);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(ex.Message); }
        })
        .RequireAuthorization();

        app.MapDelete("/documents/{documentId}/quiz/{questionId}", async (Guid documentId, Guid questionId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteQuizQuestionCommand(questionId), cancellationToken);
            return Results.NoContent();
        })
        .RequireAuthorization();

        app.MapGet("/documents/{documentId}/notes", async (Guid documentId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetNotesQuery(documentId), cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization();

        app.MapPost("/documents/{documentId}/notes", async (Guid documentId, AddNoteRequest body, ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await mediator.Send(new AddNoteCommand(documentId, user.GetUserId(), body.Content), cancellationToken);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException) { return Results.NotFound(); }
        })
        .RequireAuthorization();

        app.MapDelete("/documents/{documentId}/notes/{noteId}", async (Guid documentId, Guid noteId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteNoteCommand(noteId), cancellationToken);
            return Results.NoContent();
        })
        .RequireAuthorization();

        return app;
    }
}

public sealed record AddNoteRequest(string Content);
public sealed record GenerateFlashcardsRequest(int Count);
public sealed record GenerateQuizRequest(int Count);
