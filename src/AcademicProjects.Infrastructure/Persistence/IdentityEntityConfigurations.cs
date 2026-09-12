using AcademicProjects.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicProjects.Infrastructure.Persistence;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.LastName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.PersonalIdNumber).HasMaxLength(50).IsRequired();
        builder.Property(user => user.StudentId).HasMaxLength(50);
        builder.Property(user => user.ApprovalStatus).HasConversion<int>().IsRequired();
        builder.Property(user => user.CreatedAt).IsRequired();

        builder.HasIndex(user => user.PersonalIdNumber).IsUnique();
        builder.HasIndex(user => user.StudentId).IsUnique();
    }
}
