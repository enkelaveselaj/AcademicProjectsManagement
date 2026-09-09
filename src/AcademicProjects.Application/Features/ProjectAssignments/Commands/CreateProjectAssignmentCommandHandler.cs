using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectAssignments.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectAssignments.Commands;

public sealed class CreateProjectAssignmentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<CreateProjectAssignmentCommand, ProjectAssignmentDto>
{
    public async Task<ProjectAssignmentDto> Handle(
        CreateProjectAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var projectExists = await context.Projects
            .AnyAsync(
                project => project.Id == request.ProjectId,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(request.ProjectId, currentUser.GetUserId(), cancellationToken))
        {
            throw new ForbiddenAccessException("Only a project member or an administrator can assign users to this project.");
        }

        var assignment = new ProjectAssignment
        {
            ProjectId = request.ProjectId,
            UserId = request.UserId,
            Role = request.Role.Trim()
        };

        context.ProjectAssignments.Add(assignment);

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
