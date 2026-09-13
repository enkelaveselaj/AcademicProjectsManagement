using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Categories.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Categories.Commands;

public sealed class UpdateCategoryCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .FirstOrDefaultAsync(
                category => category.Id == request.Id,
                cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category", request.Id);
        }

        var name = request.Name.Trim();

        if (await context.Categories.AnyAsync(
                other => other.Id != category.Id && other.Name == name, cancellationToken))
        {
            throw new ConflictException($"A category named '{name}' already exists.");
        }

        category.Name = name;
        category.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        await context.SaveChangesAsync(cancellationToken);

        var projectCount = await context.Projects
            .CountAsync(project => project.CategoryId == category.Id, cancellationToken);

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            projectCount);
    }
}