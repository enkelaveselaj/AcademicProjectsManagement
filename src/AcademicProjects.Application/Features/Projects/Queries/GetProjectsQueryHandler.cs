using AcademicProjects.Application.Features.Projects.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Projects.Queries;

public sealed class GetProjectsQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetProjectsQuery, IReadOnlyList<ProjectDto>>
{
    public async Task<IReadOnlyList<ProjectDto>> Handle(
        GetProjectsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Projects
            .AsNoTracking()
            .OrderBy(project => project.Title)
            .Select(project => new ProjectDto(
                project.Id,
                project.Title,
                project.Description,
                project.Status,
                project.CategoryId,
                project.Category.Name))
            .ToListAsync(cancellationToken);
    }
}