using AcademicProjects.Application.Features.Projects.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Projects.Queries;

public sealed class GetProjectByIdQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetProjectByIdQuery, ProjectDto?>
{
    public async Task<ProjectDto?> Handle(
        GetProjectByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Projects
            .AsNoTracking()
            .Where(project => project.Id == request.Id)
            .Select(project => new ProjectDto(
                project.Id,
                project.Title,
                project.Description,
                project.Status,
                project.CategoryId,
                project.Category.Name))
            .FirstOrDefaultAsync(cancellationToken);
    }
}