using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Tests.Domain;

public class ProjectMilestoneTests
{
    [Fact]
    public void GetEffectiveStatus_PastDueAndNotCompleted_ReturnsOverdue()
    {
        var milestone = new ProjectMilestone
        {
            DueDate = DateTime.UtcNow.AddDays(-1),
            Status = MilestoneStatus.InProgress
        };

        Assert.Equal(MilestoneStatus.Overdue, milestone.GetEffectiveStatus());
    }

    [Fact]
    public void GetEffectiveStatus_PastDueButCompleted_ReturnsCompleted()
    {
        var milestone = new ProjectMilestone
        {
            DueDate = DateTime.UtcNow.AddDays(-1),
            Status = MilestoneStatus.Completed
        };

        Assert.Equal(MilestoneStatus.Completed, milestone.GetEffectiveStatus());
    }

    [Fact]
    public void GetEffectiveStatus_NotYetDue_ReturnsStoredStatus()
    {
        var milestone = new ProjectMilestone
        {
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = MilestoneStatus.Pending
        };

        Assert.Equal(MilestoneStatus.Pending, milestone.GetEffectiveStatus());
    }
}
