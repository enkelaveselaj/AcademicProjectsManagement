using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectAssignments.Commands;

public sealed class DeleteProjectAssignmentCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<DeleteProjectAssignmentCommand>
{
    public async Task Handle(
        DeleteProjectAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var assignment = await context.ProjectAssignments
            .FirstOrDefaultAsync(
                assignment => assignment.Id == request.Id,
                cancellationToken);

        if (assignment is null)
        {
            throw new NotFoundException("ProjectAssignment", request.Id);
        }

        context.ProjectAssignments.Remove(assignment);

        await context.SaveChangesAsync(cancellationToken);
    }
}
