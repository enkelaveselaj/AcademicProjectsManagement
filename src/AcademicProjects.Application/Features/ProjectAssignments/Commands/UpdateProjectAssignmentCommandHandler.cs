using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.ProjectAssignments.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectAssignments.Commands;

public sealed class UpdateProjectAssignmentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess,
    ProjectNotificationService notifier)
    : IRequestHandler<UpdateProjectAssignmentCommand, ProjectAssignmentDto>
{
    public async Task<ProjectAssignmentDto> Handle(
        UpdateProjectAssignmentCommand request,
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
            throw new ForbiddenAccessException("Only a project member or an administrator can update this assignment.");
        }

        var projectTitle = await context.Projects
            .Where(project => project.Id == request.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        if (projectTitle is null)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        var originalProjectId = assignment.ProjectId;
        var role = request.Role.Trim();

        assignment.ProjectId = request.ProjectId;
        assignment.UserId = request.UserId;
        assignment.Role = role;

        var userId = currentUser.GetUserId();

        await notifier.NotifyMembersAsync(
            originalProjectId,
            userId,
            $"An assignment on project '{projectTitle}' was updated.",
            NotificationType.Information,
            cancellationToken);

        notifier.NotifyUser(
            request.UserId,
            userId,
            $"Your assignment on project '{projectTitle}' was updated to {role}.",
            NotificationType.Information);

        await context.SaveChangesAsync(cancellationToken);

        return new ProjectAssignmentDto(
            assignment.Id,
            assignment.ProjectId,
            assignment.UserId,
            assignment.Role,
            assignment.CreatedAt,
            assignment.UpdatedAt);
    }
}
