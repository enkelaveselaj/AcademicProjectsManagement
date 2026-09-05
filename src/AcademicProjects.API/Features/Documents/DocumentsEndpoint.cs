using AcademicProjects.Application.Features.Documents.Commands;
using AcademicProjects.Application.Features.Documents.Queries;
using MediatR;

namespace AcademicProjects.API.Features.Documents;

public static class DocumentsEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/documents")
            .RequireAuthorization();

        group.MapGet("/", GetDocumentsAsync);
        group.MapGet("/{id:guid}", GetDocumentByIdAsync);
        group.MapPost("/", CreateDocumentAsync);
        group.MapPut("/{id:guid}", UpdateDocumentAsync);
        group.MapDelete("/{id:guid}", DeleteDocumentAsync);

        return endpoints;
    }

    private static async Task<IResult> GetDocumentsAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var documents = await sender.Send(
            new GetDocumentsQuery(),
            cancellationToken);

        return Results.Ok(documents);
    }

    private static async Task<IResult> GetDocumentByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var document = await sender.Send(
            new GetDocumentByIdQuery(id),
            cancellationToken);

        return document is null
            ? Results.NotFound()
            : Results.Ok(document);
    }

    private static async Task<IResult> CreateDocumentAsync(
        CreateDocumentCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var document = await sender.Send(
            command,
            cancellationToken);

        return Results.Created(
            $"/api/documents/{document.Id}",
            document);
    }

    private static async Task<IResult> UpdateDocumentAsync(
        Guid id,
        UpdateDocumentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDocumentCommand(
            id,
            request.FileName,
            request.FilePath,
            request.ProjectId);

        var document = await sender.Send(
            command,
            cancellationToken);

        return document is null
            ? Results.NotFound()
            : Results.Ok(document);
    }

    private static async Task<IResult> DeleteDocumentAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(
            new DeleteDocumentCommand(id),
            cancellationToken);

        return deleted
            ? Results.NoContent()
            : Results.NotFound();
    }
}