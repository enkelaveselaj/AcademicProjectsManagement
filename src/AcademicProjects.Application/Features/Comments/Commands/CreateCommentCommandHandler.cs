using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.Comments.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed class CreateCommentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess,
    ProjectNotificationService notifier)
    : IRequestHandler<CreateCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(
        CreateCommentCommand request,
        CancellationToken cancellationToken)
    {
        var projectTitle = await context.Projects
            .Where(project => project.Id == request.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        if (projectTitle is null)
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

        await notifier.NotifyMembersAsync(
            request.ProjectId,
            userId,
            $"New comment on project '{projectTitle}'.",
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
