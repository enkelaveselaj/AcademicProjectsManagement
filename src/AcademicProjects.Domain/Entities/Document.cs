using AcademicProjects.Domain.Common;

namespace AcademicProjects.Domain.Entities;

public class Document : AuditableEntity
{
    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;
}