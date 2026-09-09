using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed class DeleteNotificationCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteNotificationCommand>
{
    public async Task Handle(
        DeleteNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(
                n => n.Id == request.Id,
                cancellationToken);

        if (notification is null)
            throw new NotFoundException("Notification", request.Id);

        if (!currentUser.IsAdministrator() && notification.UserId != currentUser.GetUserId())
        {
            throw new ForbiddenAccessException("Only the recipient or an administrator can delete this notification.");
        }

        context.Notifications.Remove(notification);

        await context.SaveChangesAsync(cancellationToken);
    }
}
