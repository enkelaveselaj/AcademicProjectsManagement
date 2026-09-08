using AcademicProjects.Application.Features.ProjectAssignments.Commands;
using AcademicProjects.Application.Features.ProjectAssignments.Queries;
using MediatR;

namespace AcademicProjects.API.Features.ProjectAssignments;

public static class ProjectAssignmentsEndpoints
{
    public static IEndpointRouteBuilder MapProjectAssignmentEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/project-assignments")
            .RequireAuthorization();

        group.MapGet("/", GetProjectAssignmentsAsync);
        group.MapGet("/{id:guid}", GetProjectAssignmentByIdAsync);
        group.MapPost("/", CreateProjectAssignmentAsync);
        group.MapPut("/{id:guid}", UpdateProjectAssignmentAsync);
        group.MapDelete("/{id:guid}", DeleteProjectAssignmentAsync);

        return endpoints;
    }

    private static async Task<IResult> GetProjectAssignmentsAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var assignments = await sender.Send(
            new GetProjectAssignmentsQuery(),
            cancellationToken);

        return Results.Ok(assignments);
    }

    private static async Task<IResult> GetProjectAssignmentByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var assignment = await sender.Send(
            new GetProjectAssignmentByIdQuery(id),
            cancellationToken);

        return Results.Ok(assignment);
    }

    private static async Task<IResult> CreateProjectAssignmentAsync(
        CreateProjectAssignmentCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var assignment = await sender.Send(
            command,
            cancellationToken);

        return Results.Created(
            $"/api/project-assignments/{assignment.Id}",
            assignment);
    }

    private static async Task<IResult> UpdateProjectAssignmentAsync(
        Guid id,
        UpdateProjectAssignmentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProjectAssignmentCommand(
            id,
            request.ProjectId,
            request.UserId,
            request.Role);

        var assignment = await sender.Send(
            command,
            cancellationToken);

        return Results.Ok(assignment);
    }

    private static async Task<IResult> DeleteProjectAssignmentAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new DeleteProjectAssignmentCommand(id),
            cancellationToken);

        return Results.NoContent();
    }
}
