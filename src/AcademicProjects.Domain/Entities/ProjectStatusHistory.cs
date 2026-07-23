using AcademicProjects.Domain.Common;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Domain.Entities;

public class ProjectStatusHistory : AuditableEntity
{
    public ProjectStatus PreviousStatus { get; set; }


    public ProjectStatus NewStatus { get; set; }


    public string? Comment { get; set; }


    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;
}