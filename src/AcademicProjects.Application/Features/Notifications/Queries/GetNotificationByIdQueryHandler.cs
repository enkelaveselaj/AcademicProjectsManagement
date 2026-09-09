using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Notifications.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Notifications.Queries;

public sealed class GetNotificationByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<GetNotificationByIdQuery, NotificationDto>
{
    public async Task<NotificationDto> Handle(
        GetNotificationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var notification = await context.Notifications
            .AsNoTracking()
            .Where(n => n.Id == request.Id)
            .Select(n => new NotificationDto(
                n.Id,
                n.Message,
                n.Type,
                n.IsRead,
                n.UserId,
                n.CreatedAt,
                n.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (notification is null)
        {
            throw new NotFoundException("Notification", request.Id);
        }

        if (!currentUser.IsAdministrator() && notification.UserId != currentUser.GetUserId())
        {
            throw new ForbiddenAccessException("You do not have access to this notification.");
        }

        return notification;
    }
}
