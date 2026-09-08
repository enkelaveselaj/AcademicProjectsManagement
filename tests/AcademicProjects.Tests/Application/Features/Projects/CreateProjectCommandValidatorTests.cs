using AcademicProjects.Application.Features.Projects.Commands;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Tests.Application.Features.Projects;

public class CreateProjectCommandValidatorTests
{
    private readonly CreateProjectCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new CreateProjectCommand("Title", "Description", ProjectStatus.Draft, Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyTitle_HasError()
    {
        var result = _validator.Validate(
            new CreateProjectCommand("", "Description", ProjectStatus.Draft, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectCommand.Title));
    }

    [Fact]
    public void Validate_EmptyCategoryId_HasError()
    {
        var result = _validator.Validate(
            new CreateProjectCommand("Title", "Description", ProjectStatus.Draft, Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectCommand.CategoryId));
    }

    [Fact]
    public void Validate_InvalidStatus_HasError()
    {
        var result = _validator.Validate(
            new CreateProjectCommand("Title", "Description", (ProjectStatus)999, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectCommand.Status));
    }
}
