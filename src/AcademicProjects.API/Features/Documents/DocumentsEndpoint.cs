using AcademicProjects.Application.Features.Documents.Commands;
using AcademicProjects.Application.Features.Documents.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AcademicProjects.API.Features.Documents;

public static class DocumentEndpoint
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/documents")
            .RequireAuthorization();

        group.MapGet("/", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var documents = await sender.Send(
                new GetDocumentsQuery(),
                cancellationToken);

            return Results.Ok(documents);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var document = await sender.Send(
                new GetDocumentByIdQuery(id),
                cancellationToken);

            return document is null
                ? Results.NotFound()
                : Results.Ok(document);
        });

        group.MapPost("/", async (
            CreateDocumentCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var document = await sender.Send(
                command,
                cancellationToken);

            return Results.Created(
                $"/api/documents/{document.Id}",
                document);
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateDocumentCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            if (id != command.Id)
            {
                return Results.BadRequest(
                    "Route ID and body ID must match.");
            }

            var document = await sender.Send(
                command,
                cancellationToken);

            return document is null
                ? Results.NotFound()
                : Results.Ok(document);
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var deleted = await sender.Send(
                new DeleteDocumentCommand(id),
                cancellationToken);

            return deleted
                ? Results.NoContent()
                : Results.NotFound();
        });

        return endpoints;
    }
}