using AcademicProjects.Domain.Common;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Domain.Entities;

public class Notification : AuditableEntity
{
    public string Message { get; set; } = string.Empty;


    public NotificationType Type { get; set; }


    public bool IsRead { get; set; }


    public Guid UserId { get; set; }
}