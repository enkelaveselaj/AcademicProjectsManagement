using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Comments.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed class CreateCommentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<CreateCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(
        CreateCommentCommand request,
        CancellationToken cancellationToken)
    {
        var projectExists = await context.Projects
            .AnyAsync(
                project => project.Id == request.ProjectId,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        var userId = currentUser.GetUserId();

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(request.ProjectId, userId, cancellationToken))
        {
            throw new ForbiddenAccessException("Only members of this project can comment on it.");
        }

        var comment = new Comment
        {
            Content = request.Content.Trim(),
            AuthorId = userId,
            ProjectId = request.ProjectId
        };

        context.Comments.Add(comment);

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
