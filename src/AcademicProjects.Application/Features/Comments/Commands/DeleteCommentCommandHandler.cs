using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed class DeleteCommentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
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

        context.Comments.Remove(comment);

        await context.SaveChangesAsync(cancellationToken);
    }
}