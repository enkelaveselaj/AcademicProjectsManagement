using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.ProjectInvitations.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectInvitations.Commands;

public sealed class AcceptProjectInvitationCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess,
    ProjectNotificationService notifier)
    : IRequestHandler<AcceptProjectInvitationCommand, ProjectInvitationDto>
{
    public async Task<ProjectInvitationDto> Handle(
        AcceptProjectInvitationCommand request,
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
            throw new ForbiddenAccessException("Only the invited user or an administrator can accept this invitation.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new ConflictException("This invitation is no longer pending.");
        }

        if (await projectAccess.IsMemberAsync(invitation.ProjectId, invitation.InvitedUserId, cancellationToken))
        {
            throw new ConflictException("This user is already a member of the project.");
        }

        if (invitation.Role == nameof(UserRole.Mentor))
        {
            var hasMentor = await context.ProjectAssignments
                .AnyAsync(
                    assignment => assignment.ProjectId == invitation.ProjectId && assignment.Role == nameof(UserRole.Mentor),
                    cancellationToken);

            if (hasMentor)
            {
                throw new ConflictException("This project already has a mentor.");
            }
        }

        var projectTitle = await context.Projects
            .Where(project => project.Id == invitation.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        context.ProjectAssignments.Add(new ProjectAssignment
        {
            ProjectId = invitation.ProjectId,
            UserId = invitation.InvitedUserId,
            Role = invitation.Role
        });

        invitation.Status = InvitationStatus.Accepted;

        await notifier.NotifyMembersAsync(
            invitation.ProjectId,
            actorId,
            $"A new {invitation.Role} joined project '{projectTitle}'.",
            NotificationType.Information,
            cancellationToken);

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
