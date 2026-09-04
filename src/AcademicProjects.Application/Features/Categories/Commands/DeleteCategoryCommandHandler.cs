using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Categories.Commands;

public sealed class DeleteCategoryCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<DeleteCategoryCommand, bool>
{
    public async Task<bool> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .FirstOrDefaultAsync(
                category => category.Id == request.Id,
                cancellationToken);

        if (category is null)
        {
            return false;
        }

        context.Categories.Remove(category);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}