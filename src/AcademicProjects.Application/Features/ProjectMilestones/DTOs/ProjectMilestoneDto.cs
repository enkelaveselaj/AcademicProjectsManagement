namespace AcademicProjects.Application.Features.ProjectMilestones.DTOs;

public sealed record ProjectMilestoneDto(
    Guid Id,
    string Title,
    Guid ProjectId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
