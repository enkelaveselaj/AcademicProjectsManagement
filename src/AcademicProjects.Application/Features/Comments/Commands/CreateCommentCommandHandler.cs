using AcademicProjects.Application.Features.Comments.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed class CreateCommentCommandHandler(
    IApplicationDbContext context)
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
            throw new KeyNotFoundException("Project not found.");
        }

        var comment = new Comment
        {
            Content = request.Content.Trim(),
            ProjectId = request.ProjectId
        };

        context.Comments.Add(comment);

        await context.SaveChangesAsync(cancellationToken);

        return new CommentDto(
            comment.Id,
            comment.Content,
            comment.ProjectId,
            comment.CreatedAt,
            comment.UpdatedAt);
    }
}