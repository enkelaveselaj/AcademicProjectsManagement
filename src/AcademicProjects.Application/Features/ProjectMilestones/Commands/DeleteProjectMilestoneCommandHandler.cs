using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectMilestones.Commands;

public sealed class DeleteProjectMilestoneCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<DeleteProjectMilestoneCommand>
{
    public async Task Handle(
        DeleteProjectMilestoneCommand request,
        CancellationToken cancellationToken)
    {
        var milestone = await context.ProjectMilestones
            .FirstOrDefaultAsync(
                milestone => milestone.Id == request.Id,
                cancellationToken);

        if (milestone is null)
        {
            throw new NotFoundException("ProjectMilestone", request.Id);
        }

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(milestone.ProjectId, currentUser.GetUserId(), cancellationToken))
        {
            throw new ForbiddenAccessException("Only a project member or an administrator can delete this milestone.");
        }

        context.ProjectMilestones.Remove(milestone);

        await context.SaveChangesAsync(cancellationToken);
    }
}
