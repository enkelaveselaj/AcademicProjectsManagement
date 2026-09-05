using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Application.Features.Projects.DTOs;

public sealed record ProjectDto(
    Guid Id,
    string Title,
    string? Description,
    ProjectStatus Status,
    Guid CategoryId,
    string CategoryName);