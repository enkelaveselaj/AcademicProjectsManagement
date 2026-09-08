namespace AcademicProjects.Application.Features.ProjectAssignments.DTOs;

public sealed record ProjectAssignmentDto(
    Guid Id,
    Guid ProjectId,
    Guid UserId,
    string Role,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
