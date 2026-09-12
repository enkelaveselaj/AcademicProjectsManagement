using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.Projects.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Projects.Commands;

public sealed class UpdateProjectCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess,
    ProjectNotificationService notifier)
    : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await context.Projects
            .FirstOrDefaultAsync(
                project => project.Id == request.Id,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("Project", request.Id);
        }

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(project.Id, currentUser.GetUserId(), cancellationToken))
        {
            throw new ForbiddenAccessException("Only a project member or an administrator can update this project.");
        }

        var category = await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(
                category => category.Id == request.CategoryId,
                cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category", request.CategoryId);
        }

        var previousStatus = project.Status;

        project.Title = request.Title.Trim();

        project.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        project.Status = request.Status;
        project.CategoryId = request.CategoryId;

        var userId = currentUser.GetUserId();

        if (previousStatus != request.Status)
        {
            context.ProjectStatusHistories.Add(new ProjectStatusHistory
            {
                ProjectId = project.Id,
                PreviousStatus = previousStatus,
                NewStatus = request.Status,
                Comment = string.IsNullOrWhiteSpace(request.StatusChangeComment)
                    ? null
                    : request.StatusChangeComment.Trim()
            });

            await notifier.NotifyMembersAsync(
                project.Id,
                userId,
                $"Project '{project.Title}' status changed from {previousStatus} to {request.Status}.",
                NotificationType.Information,
                cancellationToken);
        }
        else
        {
            await notifier.NotifyMembersAsync(
                project.Id,
                userId,
                $"Project '{project.Title}' was updated.",
                NotificationType.Information,
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new ProjectDto(
            project.Id,
            project.Title,
            project.Description,
            project.Status,
            project.CategoryId,
            category.Name,
            project.CreatedById,
            project.CreatedAt,
            project.UpdatedAt);
    }
}