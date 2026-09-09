using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectStatusHistories.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectStatusHistories.Queries;

public sealed class GetProjectStatusHistoryByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
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

        if (history is null)
        {
            throw new NotFoundException("ProjectStatusHistory", request.Id);
        }

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(history.ProjectId, currentUser.GetUserId(), cancellationToken))
        {
            throw new ForbiddenAccessException("You do not have access to this project's status history.");
        }

        return history;
    }
}
