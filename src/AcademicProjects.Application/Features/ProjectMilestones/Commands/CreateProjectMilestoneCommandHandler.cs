using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectMilestones.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectMilestones.Commands;

public sealed class CreateProjectMilestoneCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<CreateProjectMilestoneCommand, ProjectMilestoneDto>
{
    public async Task<ProjectMilestoneDto> Handle(
        CreateProjectMilestoneCommand request,
        CancellationToken cancellationToken)
    {
        var projectExists = await context.Projects
            .AnyAsync(
                project => project.Id == request.ProjectId,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(request.ProjectId, currentUser.GetUserId(), cancellationToken))
        {
            throw new ForbiddenAccessException("Only a project member or an administrator can create milestones.");
        }

        var milestone = new ProjectMilestone
        {
            Title = request.Title.Trim(),
            ProjectId = request.ProjectId
        };

        context.ProjectMilestones.Add(milestone);

        await context.SaveChangesAsync(cancellationToken);

        return new ProjectMilestoneDto(
            milestone.Id,
            milestone.Title,
            milestone.ProjectId,
            milestone.CreatedAt,
            milestone.UpdatedAt);
    }
}
