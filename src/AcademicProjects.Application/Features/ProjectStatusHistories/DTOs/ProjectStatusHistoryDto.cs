using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Application.Features.ProjectStatusHistories.DTOs;

public sealed record ProjectStatusHistoryDto(
    Guid Id,
    Guid ProjectId,
    ProjectStatus PreviousStatus,
    ProjectStatus NewStatus,
    string? Comment,
    DateTime CreatedAt);
