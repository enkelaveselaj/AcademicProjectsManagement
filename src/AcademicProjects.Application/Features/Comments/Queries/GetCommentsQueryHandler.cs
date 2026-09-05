using AcademicProjects.Application.Features.Comments.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Comments.Queries;

public sealed class GetCommentsQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetCommentsQuery, IReadOnlyList<CommentDto>>
{
    public async Task<IReadOnlyList<CommentDto>> Handle(
        GetCommentsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Comments
            .AsNoTracking()
            .OrderByDescending(comment => comment.CreatedAt)
            .Select(comment => new CommentDto(
                comment.Id,
                comment.Content,
                comment.ProjectId,
                comment.CreatedAt,
                comment.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}