using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.ProjectStatusHistories.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectStatusHistories.Queries;

public sealed class GetProjectStatusHistoriesQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<GetProjectStatusHistoriesQuery, IReadOnlyList<ProjectStatusHistoryDto>>
{
    public async Task<IReadOnlyList<ProjectStatusHistoryDto>> Handle(
        GetProjectStatusHistoriesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.ProjectStatusHistories.AsNoTracking();

        if (!currentUser.IsAdministrator())
        {
            var accessibleProjectIds = await projectAccess.GetAccessibleProjectIdsAsync(
                currentUser.GetUserId(),
                cancellationToken);

            query = query.Where(history => accessibleProjectIds.Contains(history.ProjectId));
        }

        return await query
            .OrderByDescending(history => history.CreatedAt)
            .Select(history => new ProjectStatusHistoryDto(
                history.Id,
                history.ProjectId,
                history.PreviousStatus,
                history.NewStatus,
                history.Comment,
                history.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
