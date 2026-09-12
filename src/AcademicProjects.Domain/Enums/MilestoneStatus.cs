namespace AcademicProjects.Domain.Enums;

public enum MilestoneStatus
{
    Pending = 1,

    InProgress = 2,

    Completed = 3,

    /// <summary>
    /// Never stored directly - computed at read time when a non-completed milestone's
    /// due date has passed. Clients cannot set this value through the API.
    /// </summary>
    Overdue = 4
}
