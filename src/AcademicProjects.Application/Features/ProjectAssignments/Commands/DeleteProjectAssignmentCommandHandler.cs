using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectAssignments.Commands;

public sealed class DeleteProjectAssignmentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
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

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(assignment.ProjectId, currentUser.GetUserId(), cancellationToken))
        {
            throw new ForbiddenAccessException("Only a project member or an administrator can remove this assignment.");
        }

        context.ProjectAssignments.Remove(assignment);

        await context.SaveChangesAsync(cancellationToken);
    }
}
