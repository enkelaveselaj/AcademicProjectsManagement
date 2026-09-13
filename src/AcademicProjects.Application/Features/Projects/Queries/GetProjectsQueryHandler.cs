using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.Projects.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Projects.Queries;

public sealed class GetProjectsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<GetProjectsQuery, IReadOnlyList<ProjectDto>>
{
    public async Task<IReadOnlyList<ProjectDto>> Handle(
        GetProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Projects.AsNoTracking();

        if (!currentUser.IsAdministrator())
        {
            var accessibleProjectIds = await projectAccess.GetAccessibleProjectIdsAsync(
                currentUser.GetUserId(),
                cancellationToken);

            query = query.Where(project => accessibleProjectIds.Contains(project.Id));
        }

        return await query
            .OrderBy(project => project.Title)
            .Select(project => new ProjectDto(
                project.Id,
                project.Title,
                project.Description,
                project.Status,
                project.CategoryId,
                project.Category.Name,
                project.CreatedById,
                project.CreatedAt,
                project.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
