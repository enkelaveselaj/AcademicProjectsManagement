using AcademicProjects.Domain.Common;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Domain.Entities;

public class Project : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; }
        = ProjectStatus.Draft;

    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public ICollection<ProjectMilestone> Milestones { get; set; }
        = new List<ProjectMilestone>();

    public ICollection<Document> Documents { get; set; }
        = new List<Document>();

    public ICollection<Comment> Comments { get; set; }
        = new List<Comment>();
}