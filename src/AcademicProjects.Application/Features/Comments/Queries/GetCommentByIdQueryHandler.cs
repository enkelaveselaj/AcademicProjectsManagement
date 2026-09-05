using AcademicProjects.Application.Features.Comments.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Queries;

public sealed class GetCommentByIdQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetCommentByIdQuery, CommentDto?>
{
    public async Task<CommentDto?> Handle(
        GetCommentByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Comments
            .AsNoTracking()
            .Where(comment => comment.Id == request.Id)
            .Select(comment => new CommentDto(
                comment.Id,
                comment.Content,
                comment.ProjectId,
                comment.CreatedAt,
                comment.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}