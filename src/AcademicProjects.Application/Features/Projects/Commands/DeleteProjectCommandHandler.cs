using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Projects.Commands;

public sealed class DeleteProjectCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<DeleteProjectCommand>
{
    public async Task Handle(
        DeleteProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await context.Projects
            .FirstOrDefaultAsync(
                project => project.Id == request.Id,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("Project", request.Id);
        }

        context.Projects.Remove(project);

        await context.SaveChangesAsync(cancellationToken);
    }
}
