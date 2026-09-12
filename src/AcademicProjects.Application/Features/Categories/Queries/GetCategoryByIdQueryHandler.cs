using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Categories.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Categories.Queries;

public sealed class GetCategoryByIdQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    public async Task<CategoryDto> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .AsNoTracking()
            .Where(category => category.Id == request.Id)
            .Select(category => new CategoryDto(
                category.Id,
                category.Name,
                category.Description,
                category.Projects.Count))
            .FirstOrDefaultAsync(cancellationToken);

        return category ?? throw new NotFoundException("Category", request.Id);
    }
}