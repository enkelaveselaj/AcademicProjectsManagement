using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.ProjectAssignments.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectAssignments.Commands;

public sealed class CreateProjectAssignmentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess,
    ProjectNotificationService notifier)
    : IRequestHandler<CreateProjectAssignmentCommand, ProjectAssignmentDto>
{
    public async Task<ProjectAssignmentDto> Handle(
        CreateProjectAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var projectTitle = await context.Projects
            .Where(project => project.Id == request.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        if (projectTitle is null)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(request.ProjectId, currentUser.GetUserId(), cancellationToken))
        {
            throw new ForbiddenAccessException("Only a project member or an administrator can assign users to this project.");
        }

        var role = request.Role.Trim();

        var assignment = new ProjectAssignment
        {
            ProjectId = request.ProjectId,
            UserId = request.UserId,
            Role = role
        };

        context.ProjectAssignments.Add(assignment);

        var userId = currentUser.GetUserId();

        await notifier.NotifyMembersAsync(
            request.ProjectId,
            userId,
            $"A new {role} was added to project '{projectTitle}'.",
            NotificationType.Information,
            cancellationToken);

        notifier.NotifyUser(
            request.UserId,
            userId,
            $"You were added to project '{projectTitle}' as {role}.",
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
