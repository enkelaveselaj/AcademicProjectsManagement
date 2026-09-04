using AcademicProjects.Domain.Common;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AcademicProjects.Application.Interfaces;

namespace AcademicProjects.Infrastructure.Persistence;

public class ApplicationDbContext 
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>,
    IApplicationDbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();

    public DbSet<ProjectMilestone> ProjectMilestones => Set<ProjectMilestone>();

    public DbSet<ProjectStatusHistory> ProjectStatusHistories => Set<ProjectStatusHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        SetAuditProperties();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditProperties();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditProperties()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
