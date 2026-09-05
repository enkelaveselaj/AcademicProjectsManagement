using AcademicProjects.Application.Features.Projects.Commands;
using AcademicProjects.Application.Features.Projects.Queries;
using MediatR;

namespace AcademicProjects.API.Features.Projects;

public static class ProjectsEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/projects")
            .RequireAuthorization();

        group.MapGet("/", GetProjectsAsync);
        group.MapGet("/{id:guid}", GetProjectByIdAsync);
        group.MapPost("/", CreateProjectAsync);
        group.MapPut("/{id:guid}", UpdateProjectAsync);
        group.MapDelete("/{id:guid}", DeleteProjectAsync);

        return endpoints;
    }

    private static async Task<IResult> GetProjectsAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var projects = await sender.Send(
            new GetProjectsQuery(),
            cancellationToken);

        return Results.Ok(projects);
    }

    private static async Task<IResult> GetProjectByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var project = await sender.Send(
            new GetProjectByIdQuery(id),
            cancellationToken);

        return project is null
            ? Results.NotFound()
            : Results.Ok(project);
    }

    private static async Task<IResult> CreateProjectAsync(
        CreateProjectCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var project = await sender.Send(
            command,
            cancellationToken);

        return Results.Created(
            $"/api/projects/{project.Id}",
            project);
    }

    private static async Task<IResult> UpdateProjectAsync(
        Guid id,
        UpdateProjectCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return Results.BadRequest(new
            {
                message = "The route ID and request ID must match."
            });
        }

        var project = await sender.Send(
            command,
            cancellationToken);

        return project is null
            ? Results.NotFound()
            : Results.Ok(project);
    }

    private static async Task<IResult> DeleteProjectAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(
            new DeleteProjectCommand(id),
            cancellationToken);

        return deleted
            ? Results.NoContent()
            : Results.NotFound();
    }
}