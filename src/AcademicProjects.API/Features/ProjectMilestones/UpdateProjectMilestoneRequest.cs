using AcademicProjects.Domain.Enums;

namespace AcademicProjects.API.Features.ProjectMilestones;

public sealed record UpdateProjectMilestoneRequest(
    string Title,
    string? Description,
    DateTime DueDate,
    MilestoneStatus Status,
    Guid ProjectId);
