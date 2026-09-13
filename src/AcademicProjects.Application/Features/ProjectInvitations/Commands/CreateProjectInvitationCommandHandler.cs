using AcademicProjects.Application.Authentication;
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

public sealed class CreateProjectInvitationCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess,
    IUserManagementService userManagementService,
    ProjectNotificationService notifier)
    : IRequestHandler<CreateProjectInvitationCommand, ProjectInvitationDto>
{
    public async Task<ProjectInvitationDto> Handle(
        CreateProjectInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var role = request.Role.Trim();

        var projectTitle = await context.Projects
            .Where(project => project.Id == request.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        if (projectTitle is null)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        var actorId = currentUser.GetUserId();

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(request.ProjectId, actorId, cancellationToken))
        {
            throw new ForbiddenAccessException("Only a project member or an administrator can invite users to this project.");
        }

        if (request.InvitedUserId == actorId)
        {
            throw new ConflictException("You cannot invite yourself.");
        }

        var invitedUserRole = await userManagementService.GetUserRoleAsync(request.InvitedUserId, cancellationToken);

        if (invitedUserRole is null)
        {
            throw new NotFoundException("User", request.InvitedUserId);
        }

        if (!string.Equals(invitedUserRole, role, StringComparison.Ordinal))
        {
            throw new ConflictException($"The invited user is not registered as a {role}.");
        }

        if (await projectAccess.IsMemberAsync(request.ProjectId, request.InvitedUserId, cancellationToken))
        {
            throw new ConflictException("This user is already a member of the project.");
        }

        var hasPendingInvitation = await context.ProjectInvitations
            .AnyAsync(
                invitation => invitation.ProjectId == request.ProjectId
                    && invitation.InvitedUserId == request.InvitedUserId
                    && invitation.Status == InvitationStatus.Pending,
                cancellationToken);

        if (hasPendingInvitation)
        {
            throw new ConflictException("An invitation is already pending for this user.");
        }

        if (role == nameof(UserRole.Mentor))
        {
            var hasMentor = await context.ProjectAssignments
                .AnyAsync(
                    assignment => assignment.ProjectId == request.ProjectId && assignment.Role == nameof(UserRole.Mentor),
                    cancellationToken);

            if (hasMentor)
            {
                throw new ConflictException("This project already has a mentor.");
            }

            var hasPendingMentorRequest = await context.ProjectInvitations
                .AnyAsync(
                    invitation => invitation.ProjectId == request.ProjectId
                        && invitation.Role == nameof(UserRole.Mentor)
                        && invitation.Status == InvitationStatus.Pending,
                    cancellationToken);

            if (hasPendingMentorRequest)
            {
                throw new ConflictException("A mentor request is already pending for this project.");
            }
        }

        var invitation = new ProjectInvitation
        {
            ProjectId = request.ProjectId,
            InvitedUserId = request.InvitedUserId,
            InvitedById = actorId,
            Role = role,
            Status = InvitationStatus.Pending
        };

        context.ProjectInvitations.Add(invitation);

        notifier.NotifyUser(
            request.InvitedUserId,
            actorId,
            $"You were invited to join project '{projectTitle}' as {role}.",
            NotificationType.Information);

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
