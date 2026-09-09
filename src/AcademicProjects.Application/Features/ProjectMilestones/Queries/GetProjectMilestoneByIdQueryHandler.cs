using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectMilestones.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectMilestones.Queries;

public sealed class GetProjectMilestoneByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<GetProjectMilestoneByIdQuery, ProjectMilestoneDto>
{
    public async Task<ProjectMilestoneDto> Handle(
        GetProjectMilestoneByIdQuery request,
        CancellationToken cancellationToken)
    {
        var milestone = await context.ProjectMilestones
            .AsNoTracking()
            .Where(milestone => milestone.Id == request.Id)
            .Select(milestone => new ProjectMilestoneDto(
                milestone.Id,
                milestone.Title,
                milestone.ProjectId,
                milestone.CreatedAt,
                milestone.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (milestone is null)
        {
            throw new NotFoundException("ProjectMilestone", request.Id);
        }

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(milestone.ProjectId, currentUser.GetUserId(), cancellationToken))
        {
            throw new ForbiddenAccessException("You do not have access to this milestone.");
        }

        return milestone;
    }
}
