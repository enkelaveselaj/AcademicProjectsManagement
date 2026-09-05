namespace AcademicProjects.Application.Features.Comments.DTOs;

public sealed record CommentDto(
    Guid Id,
    string Content,
    Guid ProjectId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);