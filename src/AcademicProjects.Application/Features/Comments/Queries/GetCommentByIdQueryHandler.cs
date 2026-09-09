using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Comments.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Queries;

public sealed class GetCommentByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<GetCommentByIdQuery, CommentDto>
{
    public async Task<CommentDto> Handle(
        GetCommentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var comment = await context.Comments
            .AsNoTracking()
            .Where(comment => comment.Id == request.Id)
            .Select(comment => new CommentDto(
                comment.Id,
                comment.Content,
                comment.AuthorId,
                comment.ProjectId,
                comment.CreatedAt,
                comment.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (comment is null)
        {
            throw new NotFoundException("Comment", request.Id);
        }

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(comment.ProjectId, currentUser.GetUserId(), cancellationToken))
        {
            throw new ForbiddenAccessException("You do not have access to this comment.");
        }

        return comment;
    }
}
