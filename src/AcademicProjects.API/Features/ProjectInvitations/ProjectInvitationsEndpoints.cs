using AcademicProjects.Application.Features.ProjectInvitations.Commands;
using AcademicProjects.Application.Features.ProjectInvitations.Queries;
using MediatR;

namespace AcademicProjects.API.Features.ProjectInvitations;

public static class ProjectInvitationsEndpoints
{
    public static IEndpointRouteBuilder MapProjectInvitationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/project-invitations")
            .RequireAuthorization();

        group.MapGet("/", GetProjectInvitationsAsync);
        group.MapPost("/", CreateProjectInvitationAsync);
        group.MapPut("/{id:guid}/accept", AcceptProjectInvitationAsync);
        group.MapPut("/{id:guid}/decline", DeclineProjectInvitationAsync);
        group.MapDelete("/{id:guid}", CancelProjectInvitationAsync);

        return endpoints;
    }

    private static async Task<IResult> GetProjectInvitationsAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var invitations = await sender.Send(
            new GetProjectInvitationsQuery(),
            cancellationToken);

        return Results.Ok(invitations);
    }

    private static async Task<IResult> CreateProjectInvitationAsync(
        CreateProjectInvitationCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var invitation = await sender.Send(
            command,
            cancellationToken);

        return Results.Created(
            $"/api/project-invitations/{invitation.Id}",
            invitation);
    }

    private static async Task<IResult> AcceptProjectInvitationAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var invitation = await sender.Send(
            new AcceptProjectInvitationCommand(id),
            cancellationToken);

        return Results.Ok(invitation);
    }

    private static async Task<IResult> DeclineProjectInvitationAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var invitation = await sender.Send(
            new DeclineProjectInvitationCommand(id),
            cancellationToken);

        return Results.Ok(invitation);
    }

    private static async Task<IResult> CancelProjectInvitationAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new CancelProjectInvitationCommand(id),
            cancellationToken);

        return Results.NoContent();
    }
}
