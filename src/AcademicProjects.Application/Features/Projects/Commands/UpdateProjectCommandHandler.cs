using AcademicProjects.Application.Features.Projects.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Projects.Commands;

public sealed class UpdateProjectCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<UpdateProjectCommand, ProjectDto?>
{
    public async Task<ProjectDto?> Handle(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await context.Projects
            .FirstOrDefaultAsync(
                project => project.Id == request.Id,
                cancellationToken);

        if (project is null)
        {
            return null;
        }

        var category = await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(
                category => category.Id == request.CategoryId,
                cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Category with ID '{request.CategoryId}' was not found.");
        }

        project.Title = request.Title.Trim();

        project.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        project.Status = request.Status;
        project.CategoryId = request.CategoryId;

        await context.SaveChangesAsync(cancellationToken);

        return new ProjectDto(
            project.Id,
            project.Title,
            project.Description,
            project.Status,
            project.CategoryId,
            category.Name);
    }
}