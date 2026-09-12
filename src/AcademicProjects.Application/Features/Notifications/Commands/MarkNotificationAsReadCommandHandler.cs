using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Notifications.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed class MarkNotificationAsReadCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<MarkNotificationAsReadCommand, NotificationDto>
{
    public async Task<NotificationDto> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

        if (notification is null)
        {
            throw new NotFoundException("Notification", request.Id);
        }

        if (!currentUser.IsAdministrator() && notification.UserId != currentUser.GetUserId())
        {
            throw new ForbiddenAccessException("Only the recipient or an administrator can update this notification.");
        }

        notification.IsRead = true;

        await context.SaveChangesAsync(cancellationToken);

        return new NotificationDto(
            notification.Id,
            notification.Message,
            notification.Type,
            notification.IsRead,
            notification.UserId,
            notification.CreatedAt,
            notification.UpdatedAt);
    }
}
