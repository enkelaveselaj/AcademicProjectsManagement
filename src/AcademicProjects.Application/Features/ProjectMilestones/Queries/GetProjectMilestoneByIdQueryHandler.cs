using AcademicProjects.Application.Features.ProjectMilestones.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectMilestones.Queries;

public sealed class GetProjectMilestoneByIdQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetProjectMilestoneByIdQuery, ProjectMilestoneDto?>
{
    public async Task<ProjectMilestoneDto?> Handle(
        GetProjectMilestoneByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.ProjectMilestones
            .AsNoTracking()
            .Where(milestone => milestone.Id == request.Id)
            .Select(milestone => new ProjectMilestoneDto(
                milestone.Id,
                milestone.Title,
                milestone.ProjectId,
                milestone.CreatedAt,
                milestone.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
