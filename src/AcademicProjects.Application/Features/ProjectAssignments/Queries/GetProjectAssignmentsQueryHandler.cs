using AcademicProjects.Application.Features.ProjectAssignments.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectAssignments.Queries;

public sealed class GetProjectAssignmentsQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetProjectAssignmentsQuery, IReadOnlyList<ProjectAssignmentDto>>
{
    public async Task<IReadOnlyList<ProjectAssignmentDto>> Handle(
        GetProjectAssignmentsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.ProjectAssignments
            .AsNoTracking()
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
