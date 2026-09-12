using AcademicProjects.Application.Features.Documents.Commands;
using AcademicProjects.Application.Features.Documents.Queries;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
        group.MapGet("/{id:guid}/download", DownloadDocumentAsync);
        group.MapPost("/", CreateDocumentAsync).DisableAntiforgery();
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

        return Results.Ok(document);
    }

    private static async Task<IResult> DownloadDocumentAsync(
        Guid id,
        ISender sender,
        IFileStorageService fileStorage,
        CancellationToken cancellationToken)
    {
        var file = await sender.Send(new GetDocumentFileQuery(id), cancellationToken);
        var stream = await fileStorage.OpenReadAsync(file.StoredFileName, cancellationToken);

        return Results.File(stream, file.ContentType, file.FileName);
    }

    private static async Task<IResult> CreateDocumentAsync(
        IFormFile file,
        [FromForm] Guid projectId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var command = new CreateDocumentCommand(
            file.FileName,
            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            file.Length,
            stream,
            projectId);

        var document = await sender.Send(command, cancellationToken);

        return Results.Created($"/api/documents/{document.Id}", document);
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
            request.ProjectId);

        var document = await sender.Send(
            command,
            cancellationToken);

        return Results.Ok(document);
    }

    private static async Task<IResult> DeleteDocumentAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new DeleteDocumentCommand(id),
            cancellationToken);

        return Results.NoContent();
    }
}
