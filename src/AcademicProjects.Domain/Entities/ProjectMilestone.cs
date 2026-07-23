using AcademicProjects.Domain.Common;

namespace AcademicProjects.Domain.Entities;

public class ProjectMilestone : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;
}