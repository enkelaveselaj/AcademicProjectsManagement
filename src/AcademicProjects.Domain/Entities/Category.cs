using AcademicProjects.Domain.Common;

namespace AcademicProjects.Domain.Entities;

public class Category : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }


    public ICollection<Project> Projects { get; set; }
        = new List<Project>();
}