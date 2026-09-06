using AcademicProjects.Application.Features.Notifications.Commands;
using AcademicProjects.Application.Features.Notifications.Queries;
using AcademicProjects.Domain.Enums;
using MediatR;

namespace AcademicProjects.API.Features.Notifications;

public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/notifications")
            .RequireAuthorization();

        group.MapGet("/", GetNotificationsAsync);
        group.MapGet("/{id:guid}", GetNotificationByIdAsync);
        group.MapPost("/", CreateNotificationAsync);
        group.MapPut("/{id:guid}", UpdateNotificationAsync);
        group.MapDelete("/{id:guid}", DeleteNotificationAsync);

        return endpoints;
    }

    private static async Task<IResult> GetNotificationsAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var notifications = await sender.Send(
            new GetNotificationsQuery(),
            cancellationToken);

        return Results.Ok(notifications);
    }

    private static async Task<IResult> GetNotificationByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var notification = await sender.Send(
            new GetNotificationByIdQuery(id),
            cancellationToken);

        return notification is null
            ? Results.NotFound()
            : Results.Ok(notification);
    }

    private static async Task<IResult> CreateNotificationAsync(
        CreateNotificationCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var notification = await sender.Send(
            command,
            cancellationToken);

        return Results.Created(
            $"/api/notifications/{notification.Id}",
            notification);
    }

    private static async Task<IResult> UpdateNotificationAsync(
        Guid id,
        UpdateNotificationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateNotificationCommand(
            id,
            request.Message,
            request.Type,
            request.IsRead,
            request.UserId);

        var notification = await sender.Send(
            command,
            cancellationToken);

        return notification is null
            ? Results.NotFound()
            : Results.Ok(notification);
    }

    private static async Task<IResult> DeleteNotificationAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(
            new DeleteNotificationCommand(id),
            cancellationToken);

        return deleted
            ? Results.NoContent()
            : Results.NotFound();
    }
}
