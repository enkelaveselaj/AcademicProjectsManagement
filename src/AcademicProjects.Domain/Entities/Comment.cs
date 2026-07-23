using AcademicProjects.Domain.Common;

namespace AcademicProjects.Domain.Entities;

public class Comment : AuditableEntity
{
    public string Content { get; set; } = string.Empty;


    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;
}