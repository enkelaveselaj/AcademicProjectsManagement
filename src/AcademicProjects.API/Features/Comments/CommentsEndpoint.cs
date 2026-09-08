using AcademicProjects.Application.Features.Comments.Commands;
using AcademicProjects.Application.Features.Comments.Queries;
using MediatR;

namespace AcademicProjects.API.Features.Comments;

public static class CommentsEndpoints
{
    public static IEndpointRouteBuilder MapCommentEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/comments")
            .RequireAuthorization();

        group.MapGet("/", GetCommentsAsync);
        group.MapGet("/{id:guid}", GetCommentByIdAsync);
        group.MapPost("/", CreateCommentAsync);
        group.MapPut("/{id:guid}", UpdateCommentAsync);
        group.MapDelete("/{id:guid}", DeleteCommentAsync);

        return endpoints;
    }

    private static async Task<IResult> GetCommentsAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var comments = await sender.Send(
            new GetCommentsQuery(),
            cancellationToken);

        return Results.Ok(comments);
    }

    private static async Task<IResult> GetCommentByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var comment = await sender.Send(
            new GetCommentByIdQuery(id),
            cancellationToken);

        return Results.Ok(comment);
    }

    private static async Task<IResult> CreateCommentAsync(
        CreateCommentCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var comment = await sender.Send(
            command,
            cancellationToken);

        return Results.Created(
            $"/api/comments/{comment.Id}",
            comment);
    }

    private static async Task<IResult> UpdateCommentAsync(
        Guid id,
        UpdateCommentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCommentCommand(
            id,
            request.Content,
            request.ProjectId);

        var comment = await sender.Send(
            command,
            cancellationToken);

        return Results.Ok(comment);
    }

    private static async Task<IResult> DeleteCommentAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new DeleteCommentCommand(id),
            cancellationToken);

        return Results.NoContent();
    }
}