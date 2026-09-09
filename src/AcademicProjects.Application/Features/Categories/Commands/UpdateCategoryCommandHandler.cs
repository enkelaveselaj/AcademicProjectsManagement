using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Categories.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Categories.Commands;

public sealed class UpdateCategoryCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdministrator())
        {
            throw new ForbiddenAccessException("Only administrators can update categories.");
        }

        var category = await context.Categories
            .FirstOrDefaultAsync(
                category => category.Id == request.Id,
                cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category", request.Id);
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