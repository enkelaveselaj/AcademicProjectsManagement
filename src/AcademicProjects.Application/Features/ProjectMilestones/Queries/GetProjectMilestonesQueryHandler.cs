using AcademicProjects.Application.Features.ProjectMilestones.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectMilestones.Queries;

public sealed class GetProjectMilestonesQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetProjectMilestonesQuery, IReadOnlyList<ProjectMilestoneDto>>
{
    public async Task<IReadOnlyList<ProjectMilestoneDto>> Handle(
        GetProjectMilestonesQuery request,
        CancellationToken cancellationToken)
    {
        return await context.ProjectMilestones
            .AsNoTracking()
            .OrderByDescending(milestone => milestone.CreatedAt)
            .Select(milestone => new ProjectMilestoneDto(
                milestone.Id,
                milestone.Title,
                milestone.ProjectId,
                milestone.CreatedAt,
                milestone.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
