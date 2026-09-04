using AcademicProjects.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Document> Documents { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectAssignment> ProjectAssignments { get; }
    DbSet<ProjectMilestone> ProjectMilestones { get; }
    DbSet<ProjectStatusHistory> ProjectStatusHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}