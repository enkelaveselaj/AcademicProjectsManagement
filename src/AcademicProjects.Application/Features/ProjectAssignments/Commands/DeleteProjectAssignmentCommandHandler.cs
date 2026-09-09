using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectAssignments.Commands;

public sealed class DeleteProjectAssignmentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess,
    ProjectNotificationService notifier)
    : IRequestHandler<DeleteProjectAssignmentCommand>
{
    public async Task Handle(
        DeleteProjectAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var assignment = await context.ProjectAssignments
            .FirstOrDefaultAsync(
                assignment => assignment.Id == request.Id,
                cancellationToken);

        if (assignment is null)
        {
            throw new NotFoundException("ProjectAssignment", request.Id);
        }

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(assignment.ProjectId, currentUser.GetUserId(), cancellationToken))
        {
            throw new ForbiddenAccessException("Only a project member or an administrator can remove this assignment.");
        }

        var projectTitle = await context.Projects
            .Where(project => project.Id == assignment.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        var userId = currentUser.GetUserId();

        await notifier.NotifyMembersAsync(
            assignment.ProjectId,
            userId,
            $"A member was removed from project '{projectTitle}'.",
            NotificationType.Information,
            cancellationToken);

        notifier.NotifyUser(
            assignment.UserId,
            userId,
            $"You were removed from project '{projectTitle}'.",
            NotificationType.Warning);

        context.ProjectAssignments.Remove(assignment);

        await context.SaveChangesAsync(cancellationToken);
    }
}
