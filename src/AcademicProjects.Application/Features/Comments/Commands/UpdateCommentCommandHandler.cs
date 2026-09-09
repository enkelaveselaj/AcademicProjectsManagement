using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.Comments.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed class UpdateCommentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectNotificationService notifier)
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

        var projectTitle = await context.Projects
            .Where(project => project.Id == request.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        if (projectTitle is null)
        {
            throw new NotFoundException("Project", request.ProjectId);
        }

        comment.Content = request.Content.Trim();
        comment.ProjectId = request.ProjectId;

        await notifier.NotifyMembersAsync(
            request.ProjectId,
            currentUser.GetUserId(),
            $"A comment was updated on project '{projectTitle}'.",
            NotificationType.Information,
            cancellationToken);

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
