using AcademicProjects.Application.Features.ProjectMilestones.Commands;
using AcademicProjects.Application.Features.ProjectMilestones.Queries;
using MediatR;

namespace AcademicProjects.API.Features.ProjectMilestones;

public static class ProjectMilestonesEndpoints
{
    public static IEndpointRouteBuilder MapProjectMilestoneEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/project-milestones")
            .RequireAuthorization();

        group.MapGet("/", GetProjectMilestonesAsync);
        group.MapGet("/{id:guid}", GetProjectMilestoneByIdAsync);
        group.MapPost("/", CreateProjectMilestoneAsync);
        group.MapPut("/{id:guid}", UpdateProjectMilestoneAsync);
        group.MapDelete("/{id:guid}", DeleteProjectMilestoneAsync);

        return endpoints;
    }

    private static async Task<IResult> GetProjectMilestonesAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var milestones = await sender.Send(
            new GetProjectMilestonesQuery(),
            cancellationToken);

        return Results.Ok(milestones);
    }

    private static async Task<IResult> GetProjectMilestoneByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var milestone = await sender.Send(
            new GetProjectMilestoneByIdQuery(id),
            cancellationToken);

        return milestone is null
            ? Results.NotFound()
            : Results.Ok(milestone);
    }

    private static async Task<IResult> CreateProjectMilestoneAsync(
        CreateProjectMilestoneCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var milestone = await sender.Send(
            command,
            cancellationToken);

        return Results.Created(
            $"/api/project-milestones/{milestone.Id}",
            milestone);
    }

    private static async Task<IResult> UpdateProjectMilestoneAsync(
        Guid id,
        UpdateProjectMilestoneRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProjectMilestoneCommand(
            id,
            request.Title,
            request.ProjectId);

        var milestone = await sender.Send(
            command,
            cancellationToken);

        return milestone is null
            ? Results.NotFound()
            : Results.Ok(milestone);
    }

    private static async Task<IResult> DeleteProjectMilestoneAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(
            new DeleteProjectMilestoneCommand(id),
            cancellationToken);

        return deleted
            ? Results.NoContent()
            : Results.NotFound();
    }
}
