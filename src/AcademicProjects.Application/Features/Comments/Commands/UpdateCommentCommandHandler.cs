using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Comments.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed class UpdateCommentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(
        UpdateCommentCommand request,
        CancellationToken cancellationToken)
    {
        var comment = await context.Comments
            .FirstOrDefaultAsync(
                comment => comment.Id == request.Id,
                cancellationToken);

        if (comment is null)
        {
            throw new NotFoundException("Comment", request.Id);
        }

        if (!currentUser.IsAdministrator() && comment.AuthorId != currentUser.GetUserId())
        {
            throw new ForbiddenAccessException("Only the author or an administrator can edit this comment.");
        }

        var projectExists = await context.Projects
            .AnyAsync(
                project => project.Id == request.ProjectId,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        comment.Content = request.Content.Trim();
        comment.ProjectId = request.ProjectId;

        await context.SaveChangesAsync(cancellationToken);

        return new CommentDto(
            comment.Id,
            comment.Content,
            comment.AuthorId,
            comment.ProjectId,
            comment.CreatedAt,
            comment.UpdatedAt);
    }
}
