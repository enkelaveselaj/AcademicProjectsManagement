using AcademicProjects.Application.Features.Comments.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed class UpdateCommentCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<UpdateCommentCommand, CommentDto?>
{
    public async Task<CommentDto?> Handle(
        UpdateCommentCommand request,
        CancellationToken cancellationToken)
    {
        var comment = await context.Comments
            .FirstOrDefaultAsync(
                comment => comment.Id == request.Id,
                cancellationToken);

        if (comment is null)
        {
            return null;
        }

        var projectExists = await context.Projects
            .AnyAsync(
                project => project.Id == request.ProjectId,
                cancellationToken);

        if (!projectExists)
        {
            throw new KeyNotFoundException("Project not found.");
        }

        comment.Content = request.Content.Trim();
        comment.ProjectId = request.ProjectId;

        await context.SaveChangesAsync(cancellationToken);

        return new CommentDto(
            comment.Id,
            comment.Content,
            comment.ProjectId,
            comment.CreatedAt,
            comment.UpdatedAt);
    }
}