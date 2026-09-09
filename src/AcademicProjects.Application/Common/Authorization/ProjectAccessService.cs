using AcademicProjects.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Common.Authorization;

public sealed class ProjectAccessService(IApplicationDbContext context)
{
    public Task<bool> IsMemberAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken) =>
        context.ProjectAssignments.AnyAsync(
            assignment => assignment.ProjectId == projectId && assignment.UserId == userId,
            cancellationToken);

    public Task<List<Guid>> GetAccessibleProjectIdsAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        context.ProjectAssignments
            .Where(assignment => assignment.UserId == userId)
            .Select(assignment => assignment.ProjectId)
            .Distinct()
            .ToListAsync(cancellationToken);
}
