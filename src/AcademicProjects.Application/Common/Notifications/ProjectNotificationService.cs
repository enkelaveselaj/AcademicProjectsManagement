using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Common.Notifications;

/// <summary>
/// Queues Notification rows for project members. Handlers call this before their own
/// SaveChangesAsync so the notifications commit atomically with the action that caused them.
/// </summary>
public sealed class ProjectNotificationService(IApplicationDbContext context)
{
    public async Task NotifyMembersAsync(
        Guid projectId,
        Guid actorUserId,
        string message,
        NotificationType type,
        CancellationToken cancellationToken)
    {
        var memberIds = await context.ProjectAssignments
            .Where(assignment => assignment.ProjectId == projectId && assignment.UserId != actorUserId)
            .Select(assignment => assignment.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var memberId in memberIds)
        {
            context.Notifications.Add(new Notification
            {
                Message = message,
                Type = type,
                UserId = memberId
            });
        }
    }

    public void NotifyUser(
        Guid userId,
        Guid actorUserId,
        string message,
        NotificationType type)
    {
        if (userId == actorUserId)
        {
            return;
        }

        context.Notifications.Add(new Notification
        {
            Message = message,
            Type = type,
            UserId = userId
        });
    }
}
