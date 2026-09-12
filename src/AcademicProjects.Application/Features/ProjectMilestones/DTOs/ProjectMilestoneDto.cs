using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Application.Features.ProjectMilestones.DTOs;

public sealed record ProjectMilestoneDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime DueDate,
    DateTime? CompletedAt,
    MilestoneStatus Status,
    Guid ProjectId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
