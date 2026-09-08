namespace AcademicProjects.API.Features.ProjectAssignments;

public sealed record UpdateProjectAssignmentRequest(
    Guid ProjectId,
    Guid UserId,
    string Role);
