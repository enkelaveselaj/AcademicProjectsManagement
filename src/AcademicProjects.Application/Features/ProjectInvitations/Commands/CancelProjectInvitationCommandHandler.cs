using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectInvitations.Commands;

public sealed class CancelProjectInvitationCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess,
    ProjectNotificationService notifier)
    : IRequestHandler<CancelProjectInvitationCommand>
{
    public async Task Handle(
        CancelProjectInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var invitation = await context.ProjectInvitations
            .FirstOrDefaultAsync(invitation => invitation.Id == request.Id, cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("ProjectInvitation", request.Id);
        }

        var actorId = currentUser.GetUserId();

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(invitation.ProjectId, actorId, cancellationToken))
        {
            throw new ForbiddenAccessException("Only a project member or an administrator can cancel this invitation.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new ConflictException("Only a pending invitation can be cancelled.");
        }

        var projectTitle = await context.Projects
            .Where(project => project.Id == invitation.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        notifier.NotifyUser(
            invitation.InvitedUserId,
            actorId,
            $"Your invitation for project '{projectTitle}' was cancelled.",
            NotificationType.Warning);

        context.ProjectInvitations.Remove(invitation);

        await context.SaveChangesAsync(cancellationToken);
    }
}
