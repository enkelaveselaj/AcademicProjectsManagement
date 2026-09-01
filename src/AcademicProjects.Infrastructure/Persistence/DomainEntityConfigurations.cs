using AcademicProjects.Domain.Common;
using AcademicProjects.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicProjects.Infrastructure.Persistence;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Name).HasMaxLength(120).IsRequired();
        builder.Property(category => category.Description).HasMaxLength(500);
        builder.HasIndex(category => category.Name).IsUnique();
        builder.ConfigureAuditProperties();
    }
}

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(project => project.Id);
        builder.Property(project => project.Title).HasMaxLength(200).IsRequired();
        builder.Property(project => project.Description).HasMaxLength(4_000).IsRequired();
        builder.Property(project => project.Status).HasConversion<int>().IsRequired();
        builder.HasOne(project => project.Category)
            .WithMany(category => category.Projects)
            .HasForeignKey(project => project.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(project => new { project.CategoryId, project.Status });
        builder.ConfigureAuditProperties();
    }
}

public sealed class ProjectMilestoneConfiguration : IEntityTypeConfiguration<ProjectMilestone>
{
    public void Configure(EntityTypeBuilder<ProjectMilestone> builder)
    {
        builder.ToTable("ProjectMilestones");
        builder.HasKey(milestone => milestone.Id);
        builder.Property(milestone => milestone.Title).HasMaxLength(200).IsRequired();
        builder.HasOne(milestone => milestone.Project)
            .WithMany(project => project.Milestones)
            .HasForeignKey(milestone => milestone.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(milestone => milestone.ProjectId);
        builder.ConfigureAuditProperties();
    }
}

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.HasKey(document => document.Id);
        builder.Property(document => document.FileName).HasMaxLength(255).IsRequired();
        builder.Property(document => document.FilePath).HasMaxLength(1_000).IsRequired();
        builder.HasOne(document => document.Project)
            .WithMany(project => project.Documents)
            .HasForeignKey(document => document.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(document => document.ProjectId);
        builder.ConfigureAuditProperties();
    }
}

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        builder.HasKey(comment => comment.Id);
        builder.Property(comment => comment.Content).HasMaxLength(4_000).IsRequired();
        builder.HasOne(comment => comment.Project)
            .WithMany(project => project.Comments)
            .HasForeignKey(comment => comment.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(comment => comment.ProjectId);
        builder.ConfigureAuditProperties();
    }
}

public sealed class ProjectAssignmentConfiguration : IEntityTypeConfiguration<ProjectAssignment>
{
    public void Configure(EntityTypeBuilder<ProjectAssignment> builder)
    {
        builder.ToTable("ProjectAssignments");
        builder.HasKey(assignment => assignment.Id);
        builder.Property(assignment => assignment.Role).HasMaxLength(50).IsRequired();
        builder.HasOne(assignment => assignment.Project)
            .WithMany()
            .HasForeignKey(assignment => assignment.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(assignment => new { assignment.ProjectId, assignment.UserId, assignment.Role }).IsUnique();
        builder.ConfigureAuditProperties();
    }
}

public sealed class ProjectStatusHistoryConfiguration : IEntityTypeConfiguration<ProjectStatusHistory>
{
    public void Configure(EntityTypeBuilder<ProjectStatusHistory> builder)
    {
        builder.ToTable("ProjectStatusHistories");
        builder.HasKey(history => history.Id);
        builder.Property(history => history.PreviousStatus).HasConversion<int>().IsRequired();
        builder.Property(history => history.NewStatus).HasConversion<int>().IsRequired();
        builder.Property(history => history.Comment).HasMaxLength(1_000);
        builder.HasOne(history => history.Project)
            .WithMany()
            .HasForeignKey(history => history.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(history => history.ProjectId);
        builder.ConfigureAuditProperties();
    }
}

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(notification => notification.Id);
        builder.Property(notification => notification.Message).HasMaxLength(1_000).IsRequired();
        builder.Property(notification => notification.Type).HasConversion<int>().IsRequired();
        builder.Property(notification => notification.IsRead).HasDefaultValue(false);
        builder.HasIndex(notification => new { notification.UserId, notification.IsRead });
        builder.ConfigureAuditProperties();
    }
}

internal static class EntityTypeBuilderExtensions
{
    public static void ConfigureAuditProperties<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
    {
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt);
    }
}
