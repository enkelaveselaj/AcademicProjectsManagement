using AcademicProjects.Application.Features.Projects.Commands;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Tests.Application.Features.Projects;

public class UpdateProjectCommandValidatorTests
{
    private readonly UpdateProjectCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new UpdateProjectCommand(Guid.NewGuid(), "Title", "Description", ProjectStatus.Draft, Guid.NewGuid(), "Comment"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(
            new UpdateProjectCommand(Guid.Empty, "Title", "Description", ProjectStatus.Draft, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProjectCommand.Id));
    }

    [Fact]
    public void Validate_StatusChangeCommentTooLong_HasError()
    {
        var result = _validator.Validate(
            new UpdateProjectCommand(Guid.NewGuid(), "Title", "Description", ProjectStatus.Draft, Guid.NewGuid(), new string('a', 1_001)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProjectCommand.StatusChangeComment));
    }
}
