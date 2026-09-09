using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectAssignments.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectAssignments.Commands;

public sealed class UpdateProjectAssignmentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<UpdateProjectAssignmentCommand, ProjectAssignmentDto>
{
    public async Task<ProjectAssignmentDto> Handle(
        UpdateProjectAssignmentCommand request,
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
            throw new ForbiddenAccessException("Only a project member or an administrator can update this assignment.");
        }

        var projectExists = await context.Projects
            .AnyAsync(
                project => project.Id == request.ProjectId,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        assignment.ProjectId = request.ProjectId;
        assignment.UserId = request.UserId;
        assignment.Role = request.Role.Trim();

        await context.SaveChangesAsync(cancellationToken);

        return new ProjectAssignmentDto(
            assignment.Id,
            assignment.ProjectId,
            assignment.UserId,
            assignment.Role,
            assignment.CreatedAt,
            assignment.UpdatedAt);
    }
}
