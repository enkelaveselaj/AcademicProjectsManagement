namespace AcademicProjects.Application.Features.Comments.DTOs;

public sealed record CommentDto(
    Guid Id,
    string Content,
    Guid AuthorId,
    Guid ProjectId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);