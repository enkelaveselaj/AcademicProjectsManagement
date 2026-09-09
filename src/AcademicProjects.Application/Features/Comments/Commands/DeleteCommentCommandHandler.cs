using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed class DeleteCommentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectNotificationService notifier)
    : IRequestHandler<DeleteCommentCommand>
{
    public async Task Handle(
        DeleteCommentCommand request,
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
            throw new ForbiddenAccessException("Only the author or an administrator can delete this comment.");
        }

        var projectTitle = await context.Projects
            .Where(project => project.Id == comment.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        await notifier.NotifyMembersAsync(
            comment.ProjectId,
            currentUser.GetUserId(),
            $"A comment was removed from project '{projectTitle}'.",
            NotificationType.Information,
            cancellationToken);

        context.Comments.Remove(comment);

        await context.SaveChangesAsync(cancellationToken);
    }
}
