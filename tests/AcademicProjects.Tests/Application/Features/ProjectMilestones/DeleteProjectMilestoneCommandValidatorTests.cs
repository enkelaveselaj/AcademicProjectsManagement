using AcademicProjects.Application.Features.ProjectMilestones.Commands;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class DeleteProjectMilestoneCommandValidatorTests
{
    private readonly DeleteProjectMilestoneCommandValidator _validator = new();

    [Fact]
    public void Validate_NonEmptyId_IsValid()
    {
        var result = _validator.Validate(new DeleteProjectMilestoneCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(new DeleteProjectMilestoneCommand(Guid.Empty));

        Assert.False(result.IsValid);
    }
}
