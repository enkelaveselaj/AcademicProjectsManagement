using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectAssignments.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectAssignments.Queries;

public sealed class GetProjectAssignmentByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<GetProjectAssignmentByIdQuery, ProjectAssignmentDto>
{
    public async Task<ProjectAssignmentDto> Handle(
        GetProjectAssignmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var assignment = await context.ProjectAssignments
            .AsNoTracking()
            .Where(assignment => assignment.Id == request.Id)
            .Select(assignment => new ProjectAssignmentDto(
                assignment.Id,
                assignment.ProjectId,
                assignment.UserId,
                assignment.Role,
                assignment.CreatedAt,
                assignment.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (assignment is null)
        {
            throw new NotFoundException("ProjectAssignment", request.Id);
        }

        var userId = currentUser.GetUserId();

        if (!currentUser.IsAdministrator()
            && assignment.UserId != userId
            && !await projectAccess.IsMemberAsync(assignment.ProjectId, userId, cancellationToken))
        {
            throw new ForbiddenAccessException("You do not have access to this project assignment.");
        }

        return assignment;
    }
}
