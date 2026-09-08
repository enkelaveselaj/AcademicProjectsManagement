namespace AcademicProjects.API.Features.ProjectMilestones;

public sealed record UpdateProjectMilestoneRequest(
    string Title,
    Guid ProjectId);
