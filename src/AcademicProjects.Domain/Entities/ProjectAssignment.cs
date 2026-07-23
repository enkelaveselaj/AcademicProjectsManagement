using AcademicProjects.Domain.Common;

namespace AcademicProjects.Domain.Entities;

public class ProjectAssignment : AuditableEntity
{
    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;


    public Guid UserId { get; set; }

    // This references ApplicationUser from Identity,
    // but we intentionally keep it as Guid here.
    // Domain should not know about ASP.NET Identity.


    public string Role { get; set; } = string.Empty;
}