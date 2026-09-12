using AcademicProjects.Application.Features.ProjectMilestones.Commands;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class CreateProjectMilestoneCommandValidatorTests
{
    private readonly CreateProjectMilestoneCommandValidator _validator = new();
    private static readonly DateTime FutureDueDate = DateTime.UtcNow.AddDays(30);

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new CreateProjectMilestoneCommand("Title", "Description", FutureDueDate, Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyTitle_HasError()
    {
        var result = _validator.Validate(
            new CreateProjectMilestoneCommand("", "Description", FutureDueDate, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectMilestoneCommand.Title));
    }

    [Fact]
    public void Validate_EmptyProjectId_HasError()
    {
        var result = _validator.Validate(
            new CreateProjectMilestoneCommand("Title", "Description", FutureDueDate, Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectMilestoneCommand.ProjectId));
    }
}
