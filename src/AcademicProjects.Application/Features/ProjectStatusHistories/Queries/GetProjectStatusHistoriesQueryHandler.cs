using AcademicProjects.Application.Features.ProjectStatusHistories.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectStatusHistories.Queries;

public sealed class GetProjectStatusHistoriesQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetProjectStatusHistoriesQuery, IReadOnlyList<ProjectStatusHistoryDto>>
{
    public async Task<IReadOnlyList<ProjectStatusHistoryDto>> Handle(
        GetProjectStatusHistoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await context.ProjectStatusHistories
            .AsNoTracking()
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
