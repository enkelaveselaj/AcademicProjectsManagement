using AcademicProjects.Domain.Common;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Domain.Entities;

public class ProjectInvitation : AuditableEntity
{
    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public Guid InvitedUserId { get; set; }

    public Guid InvitedById { get; set; }

    // References ApplicationUser from Identity via Guid only - Domain should not know about ASP.NET Identity.

    public string Role { get; set; } = string.Empty;

    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
}
