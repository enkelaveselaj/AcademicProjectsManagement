namespace AcademicProjects.API.Features.Comments;

public sealed record UpdateCommentRequest(
    string Content,
    Guid ProjectId);