using AcademicProjects.Application.Features.Categories.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Categories.Commands;

public sealed class UpdateCategoryCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto?>
{
    public async Task<CategoryDto?> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .FirstOrDefaultAsync(
                category => category.Id == request.Id,
                cancellationToken);

        if (category is null)
        {
            return null;
        }

        category.Name = request.Name.Trim();
        category.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        await context.SaveChangesAsync(cancellationToken);

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description);
    }
}