using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed class DeleteCommentCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<DeleteCommentCommand, bool>
{
    public async Task<bool> Handle(
        DeleteCommentCommand request,
        CancellationToken cancellationToken)
    {
        var comment = await context.Comments
            .FirstOrDefaultAsync(
                comment => comment.Id == request.Id,
                cancellationToken);

        if (comment is null)
        {
            return false;
        }

        context.Comments.Remove(comment);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}