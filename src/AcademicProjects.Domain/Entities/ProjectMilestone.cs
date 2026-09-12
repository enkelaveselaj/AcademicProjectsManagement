using AcademicProjects.Domain.Common;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Domain.Entities;

public class ProjectMilestone : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;

    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    /// <summary>
    /// The status as it should be displayed: a completed milestone stays Completed, an
    /// incomplete one past its due date reads as Overdue, otherwise the stored status.
    /// </summary>
    public MilestoneStatus GetEffectiveStatus()
    {
        if (Status == MilestoneStatus.Completed)
        {
            return MilestoneStatus.Completed;
        }

        return DueDate < DateTime.UtcNow ? MilestoneStatus.Overdue : Status;
    }
}
