using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.Comments.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Queries;

public sealed class GetCommentsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<GetCommentsQuery, IReadOnlyList<CommentDto>>
{
    public async Task<IReadOnlyList<CommentDto>> Handle(
        GetCommentsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Comments.AsNoTracking();

        if (!currentUser.IsAdministrator())
        {
            var accessibleProjectIds = await projectAccess.GetAccessibleProjectIdsAsync(
                currentUser.GetUserId(),
                cancellationToken);

            query = query.Where(comment => accessibleProjectIds.Contains(comment.ProjectId));
        }

        return await query
            .OrderByDescending(comment => comment.CreatedAt)
            .Select(comment => new CommentDto(
                comment.Id,
                comment.Content,
                comment.AuthorId,
                comment.ProjectId,
                comment.CreatedAt,
                comment.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
