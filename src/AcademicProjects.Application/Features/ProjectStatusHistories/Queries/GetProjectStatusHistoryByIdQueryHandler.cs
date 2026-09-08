using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectStatusHistories.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectStatusHistories.Queries;

public sealed class GetProjectStatusHistoryByIdQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetProjectStatusHistoryByIdQuery, ProjectStatusHistoryDto>
{
    public async Task<ProjectStatusHistoryDto> Handle(
        GetProjectStatusHistoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var history = await context.ProjectStatusHistories
            .AsNoTracking()
            .Where(history => history.Id == request.Id)
            .Select(history => new ProjectStatusHistoryDto(
                history.Id,
                history.ProjectId,
                history.PreviousStatus,
                history.NewStatus,
                history.Comment,
                history.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return history ?? throw new NotFoundException("ProjectStatusHistory", request.Id);
    }
}
