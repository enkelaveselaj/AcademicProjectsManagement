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

        if (milestone is null || milestone.ProjectId != request.ProjectId)
        {
            // A ProjectId that doesn't match the milestone's actual project is treated as
            // not-found rather than validated against the target project - a milestone can't
            // be reparented to a different project through this endpoint.
            throw new NotFoundException("ProjectMilestone", request.Id);
        }

        var projectTitle = await context.Projects
            .Where(project => project.Id == milestone.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        if (request.Status == MilestoneStatus.Completed && milestone.Status != MilestoneStatus.Completed)
        {
            milestone.CompletedAt = DateTime.UtcNow;
        }
        else if (request.Status != MilestoneStatus.Completed && milestone.Status == MilestoneStatus.Completed)
        {
            milestone.CompletedAt = null;
        }

        milestone.Title = request.Title.Trim();
        milestone.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        milestone.DueDate = request.DueDate;
        milestone.Status = request.Status;

        await notifier.NotifyMembersAsync(
            milestone.ProjectId,
            currentUser.GetUserId(),
            $"Milestone updated on project '{projectTitle}': {milestone.Title}.",
            NotificationType.Information,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new ProjectMilestoneDto(
            milestone.Id,
            milestone.Title,
            milestone.Description,
            milestone.DueDate,
            milestone.CompletedAt,
            milestone.GetEffectiveStatus(),
            milestone.ProjectId,
            milestone.CreatedAt,
            milestone.UpdatedAt);
    }
}
