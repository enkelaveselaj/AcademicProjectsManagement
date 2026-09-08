using AcademicProjects.Application.Features.ProjectStatusHistories.Queries;
using MediatR;

namespace AcademicProjects.API.Features.ProjectStatusHistories;

public static class ProjectStatusHistoriesEndpoints
{
    public static IEndpointRouteBuilder MapProjectStatusHistoryEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/project-status-histories")
            .RequireAuthorization();

        group.MapGet("/", GetProjectStatusHistoriesAsync);
        group.MapGet("/{id:guid}", GetProjectStatusHistoryByIdAsync);

        return endpoints;
    }

    private static async Task<IResult> GetProjectStatusHistoriesAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var histories = await sender.Send(
            new GetProjectStatusHistoriesQuery(),
            cancellationToken);

        return Results.Ok(histories);
    }

    private static async Task<IResult> GetProjectStatusHistoryByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var history = await sender.Send(
            new GetProjectStatusHistoryByIdQuery(id),
            cancellationToken);

        return Results.Ok(history);
    }
}
