using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed class MarkAllNotificationsAsReadCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<MarkAllNotificationsAsReadCommand>
{
    public async Task Handle(
        MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        // Always scoped to the caller's own notifications, even for administrators - marking
        // everyone else's notifications as read would be a real cross-user side effect.
        var unread = await context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
