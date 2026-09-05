using AcademicProjects.Application.Features.Projects.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Projects.Commands;

public sealed class CreateProjectCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
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

        var project = new Project
        {
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            Status = request.Status,
            CategoryId = request.CategoryId
        };

        context.Projects.Add(project);

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