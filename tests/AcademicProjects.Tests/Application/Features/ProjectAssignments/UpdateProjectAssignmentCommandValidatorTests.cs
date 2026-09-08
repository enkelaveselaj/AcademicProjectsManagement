using AcademicProjects.Application.Features.ProjectAssignments.Commands;

namespace AcademicProjects.Tests.Application.Features.ProjectAssignments;

public class UpdateProjectAssignmentCommandValidatorTests
{
    private readonly UpdateProjectAssignmentCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new UpdateProjectAssignmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Student"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(
            new UpdateProjectAssignmentCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), "Student"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProjectAssignmentCommand.Id));
    }
}
