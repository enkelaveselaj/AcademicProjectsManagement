using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.ProjectMilestones.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectMilestones.Commands;

public sealed class UpdateProjectMilestoneCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess,
    ProjectNotificationService notifier)
    : IRequestHandler<UpdateProjectMilestoneCommand, ProjectMilestoneDto>
{
    public async Task<ProjectMilestoneDto> Handle(
        UpdateProjectMilestoneCommand request,
        CancellationToken cancellationToken)
    {
        var milestone = await context.ProjectMilestones
            .FirstOrDefaultAsync(
                milestone => milestone.Id == request.Id,
                cancellationToken);

        if (milestone is null)
        {
            throw new NotFoundException("ProjectMilestone", request.Id);
        }

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(milestone.ProjectId, currentUser.GetUserId(), cancellationToken))
        {
            throw new ForbiddenAccessException("Only a project member or an administrator can update this milestone.");
        }

        var projectTitle = await context.Projects
            .Where(project => project.Id == request.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        if (projectTitle is null)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        milestone.Title = request.Title.Trim();
        milestone.ProjectId = request.ProjectId;

        await notifier.NotifyMembersAsync(
            request.ProjectId,
            currentUser.GetUserId(),
            $"Milestone updated on project '{projectTitle}': {milestone.Title}.",
            NotificationType.Information,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new ProjectMilestoneDto(
            milestone.Id,
            milestone.Title,
            milestone.ProjectId,
            milestone.CreatedAt,
            milestone.UpdatedAt);
    }
}
