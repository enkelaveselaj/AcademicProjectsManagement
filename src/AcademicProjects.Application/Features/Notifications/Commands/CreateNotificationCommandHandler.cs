using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Notifications.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using MediatR;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed class CreateNotificationCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateNotificationCommand, NotificationDto>
{
    public async Task<NotificationDto> Handle(
        CreateNotificationCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdministrator() && !currentUser.IsInRole(UserRole.Mentor))
        {
            throw new ForbiddenAccessException("Only administrators or mentors can send notifications.");
        }

        var notification = new Notification
        {
            Message = request.Message,
            Type = request.Type,
            IsRead = request.IsRead,
            UserId = request.UserId
        };

        context.Notifications.Add(notification);

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
