using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.ProjectInvitations.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectInvitations.Commands;

public sealed class DeclineProjectInvitationCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectNotificationService notifier)
    : IRequestHandler<DeclineProjectInvitationCommand, ProjectInvitationDto>
{
    public async Task<ProjectInvitationDto> Handle(
        DeclineProjectInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var invitation = await context.ProjectInvitations
            .FirstOrDefaultAsync(invitation => invitation.Id == request.Id, cancellationToken);

        if (invitation is null)
        {
            throw new NotFoundException("ProjectInvitation", request.Id);
        }

        var actorId = currentUser.GetUserId();

        if (!currentUser.IsAdministrator() && invitation.InvitedUserId != actorId)
        {
            throw new ForbiddenAccessException("Only the invited user or an administrator can decline this invitation.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new ConflictException("This invitation is no longer pending.");
        }

        var projectTitle = await context.Projects
            .Where(project => project.Id == invitation.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        invitation.Status = InvitationStatus.Declined;

        notifier.NotifyUser(
            invitation.InvitedById,
            actorId,
            $"Your invitation for project '{projectTitle}' was declined.",
            NotificationType.Warning);

        await context.SaveChangesAsync(cancellationToken);

        return new ProjectInvitationDto(
            invitation.Id,
            invitation.ProjectId,
            invitation.InvitedUserId,
            invitation.InvitedById,
            invitation.Role,
            invitation.Status,
            invitation.CreatedAt,
            invitation.UpdatedAt);
    }
}
