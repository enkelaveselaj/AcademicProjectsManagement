using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.ProjectInvitations.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.ProjectInvitations.Queries;

public sealed class GetProjectInvitationsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<GetProjectInvitationsQuery, IReadOnlyList<ProjectInvitationDto>>
{
    public async Task<IReadOnlyList<ProjectInvitationDto>> Handle(
        GetProjectInvitationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.ProjectInvitations.AsNoTracking();

        if (!currentUser.IsAdministrator())
        {
            var userId = currentUser.GetUserId();

            var accessibleProjectIds = await projectAccess.GetAccessibleProjectIdsAsync(
                userId,
                cancellationToken);

            query = query.Where(invitation =>
                invitation.InvitedUserId == userId
                || accessibleProjectIds.Contains(invitation.ProjectId));
        }

        return await query
            .OrderByDescending(invitation => invitation.CreatedAt)
            .Select(invitation => new ProjectInvitationDto(
                invitation.Id,
                invitation.ProjectId,
                invitation.InvitedUserId,
                invitation.InvitedById,
                invitation.Role,
                invitation.Status,
                invitation.CreatedAt,
                invitation.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
