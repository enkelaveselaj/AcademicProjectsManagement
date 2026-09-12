using AcademicProjects.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace AcademicProjects.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string PersonalIdNumber { get; set; } = string.Empty;

    /// <summary>
    /// Only set for Student accounts; null for Mentor/Administrator accounts.
    /// </summary>
    public string? StudentId { get; set; }

    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
