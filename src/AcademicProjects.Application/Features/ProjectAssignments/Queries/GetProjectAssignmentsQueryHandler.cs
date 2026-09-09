using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.ProjectAssignments.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectAssignments.Queries;

public sealed class GetProjectAssignmentsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<GetProjectAssignmentsQuery, IReadOnlyList<ProjectAssignmentDto>>
{
    public async Task<IReadOnlyList<ProjectAssignmentDto>> Handle(
        GetProjectAssignmentsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.ProjectAssignments.AsNoTracking();

        if (!currentUser.IsAdministrator())
        {
            var accessibleProjectIds = await projectAccess.GetAccessibleProjectIdsAsync(
                currentUser.GetUserId(),
                cancellationToken);

            query = query.Where(assignment => accessibleProjectIds.Contains(assignment.ProjectId));
        }

        return await query
            .OrderByDescending(assignment => assignment.CreatedAt)
            .Select(assignment => new ProjectAssignmentDto(
                assignment.Id,
                assignment.ProjectId,
                assignment.UserId,
                assignment.Role,
                assignment.CreatedAt,
                assignment.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
