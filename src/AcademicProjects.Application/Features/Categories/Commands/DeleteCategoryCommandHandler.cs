using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Categories.Commands;

public sealed class DeleteCategoryCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdministrator())
        {
            throw new ForbiddenAccessException("Only administrators can delete categories.");
        }

        var category = await context.Categories
            .FirstOrDefaultAsync(
                category => category.Id == request.Id,
                cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category", request.Id);
        }

        context.Categories.Remove(category);

        await context.SaveChangesAsync(cancellationToken);
    }
}