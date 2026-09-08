using AcademicProjects.Application.Features.ProjectMilestones.Commands;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class UpdateProjectMilestoneCommandValidatorTests
{
    private readonly UpdateProjectMilestoneCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new UpdateProjectMilestoneCommand(Guid.NewGuid(), "Title", Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(
            new UpdateProjectMilestoneCommand(Guid.Empty, "Title", Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProjectMilestoneCommand.Id));
    }
}
