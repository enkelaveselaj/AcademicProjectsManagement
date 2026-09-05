using AcademicProjects.Domain.Enums;

namespace AcademicProjects.API.Features.Projects;

public sealed record UpdateProjectRequest(
    string Title,
    string? Description,
    ProjectStatus Status,
    Guid CategoryId);