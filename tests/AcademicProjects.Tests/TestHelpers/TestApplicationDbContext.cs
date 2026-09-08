using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Tests.TestHelpers;

public sealed class TestApplicationDbContext(
    DbContextOptions<TestApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();

    public DbSet<ProjectMilestone> ProjectMilestones => Set<ProjectMilestone>();

    public DbSet<ProjectStatusHistory> ProjectStatusHistories => Set<ProjectStatusHistory>();
}
