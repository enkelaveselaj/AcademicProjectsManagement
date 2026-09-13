using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Categories.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Categories.Commands;

public sealed class CreateCategoryCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (await context.Categories.AnyAsync(category => category.Name == name, cancellationToken))
        {
            throw new ConflictException($"A category named '{name}' already exists.");
        }

        var category = new Category
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim()
        };

        context.Categories.Add(category);

        await context.SaveChangesAsync(cancellationToken);

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            ProjectCount: 0);
    }
}