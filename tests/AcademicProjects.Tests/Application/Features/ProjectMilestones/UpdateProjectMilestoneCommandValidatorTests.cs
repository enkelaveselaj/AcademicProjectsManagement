using AcademicProjects.Application.Features.ProjectMilestones.Commands;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class UpdateProjectMilestoneCommandValidatorTests
{
    private readonly UpdateProjectMilestoneCommandValidator _validator = new();
    private static readonly DateTime FutureDueDate = DateTime.UtcNow.AddDays(30);

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new UpdateProjectMilestoneCommand(Guid.NewGuid(), "Title", "Description", FutureDueDate, MilestoneStatus.InProgress, Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(
            new UpdateProjectMilestoneCommand(Guid.Empty, "Title", "Description", FutureDueDate, MilestoneStatus.Pending, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProjectMilestoneCommand.Id));
    }

    [Fact]
    public void Validate_OverdueStatus_HasError()
    {
        var result = _validator.Validate(
            new UpdateProjectMilestoneCommand(Guid.NewGuid(), "Title", "Description", FutureDueDate, MilestoneStatus.Overdue, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProjectMilestoneCommand.Status));
    }
}
