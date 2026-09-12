using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.ProjectMilestones.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectMilestones.Queries;

public sealed class GetProjectMilestonesQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<GetProjectMilestonesQuery, IReadOnlyList<ProjectMilestoneDto>>
{
    public async Task<IReadOnlyList<ProjectMilestoneDto>> Handle(
        GetProjectMilestonesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.ProjectMilestones.AsNoTracking();

        if (!currentUser.IsAdministrator())
        {
            var accessibleProjectIds = await projectAccess.GetAccessibleProjectIdsAsync(
                currentUser.GetUserId(),
                cancellationToken);

            query = query.Where(milestone => accessibleProjectIds.Contains(milestone.ProjectId));
        }

        var milestones = await query
            .OrderByDescending(milestone => milestone.CreatedAt)
            .ToListAsync(cancellationToken);

        return milestones
            .Select(milestone => new ProjectMilestoneDto(
                milestone.Id,
                milestone.Title,
                milestone.Description,
                milestone.DueDate,
                milestone.CompletedAt,
                milestone.GetEffectiveStatus(),
                milestone.ProjectId,
                milestone.CreatedAt,
                milestone.UpdatedAt))
            .ToList();
    }
}
