using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Categories.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using MediatR;

namespace AcademicProjects.Application.Features.Categories.Commands;

public sealed class CreateCategoryCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdministrator())
        {
            throw new ForbiddenAccessException("Only administrators can create categories.");
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim()
        };

        context.Categories.Add(category);

        await context.SaveChangesAsync(cancellationToken);

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description);
    }
}