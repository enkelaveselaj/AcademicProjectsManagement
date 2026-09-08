using AcademicProjects.Application.Features.ProjectAssignments.Commands;

namespace AcademicProjects.Tests.Application.Features.ProjectAssignments;

public class CreateProjectAssignmentCommandValidatorTests
{
    private readonly CreateProjectAssignmentCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new CreateProjectAssignmentCommand(Guid.NewGuid(), Guid.NewGuid(), "Student"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyProjectId_HasError()
    {
        var result = _validator.Validate(
            new CreateProjectAssignmentCommand(Guid.Empty, Guid.NewGuid(), "Student"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectAssignmentCommand.ProjectId));
    }

    [Fact]
    public void Validate_EmptyUserId_HasError()
    {
        var result = _validator.Validate(
            new CreateProjectAssignmentCommand(Guid.NewGuid(), Guid.Empty, "Student"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectAssignmentCommand.UserId));
    }

    [Fact]
    public void Validate_EmptyRole_HasError()
    {
        var result = _validator.Validate(
            new CreateProjectAssignmentCommand(Guid.NewGuid(), Guid.NewGuid(), ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectAssignmentCommand.Role));
    }

    [Fact]
    public void Validate_RoleTooLong_HasError()
    {
        var result = _validator.Validate(
            new CreateProjectAssignmentCommand(Guid.NewGuid(), Guid.NewGuid(), new string('a', 51)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectAssignmentCommand.Role));
    }
}
