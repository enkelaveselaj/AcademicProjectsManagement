using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed class DeleteCommentCommandHandler(
    IApplicationDbContext context)
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

        context.Comments.Remove(comment);

        await context.SaveChangesAsync(cancellationToken);
    }
}