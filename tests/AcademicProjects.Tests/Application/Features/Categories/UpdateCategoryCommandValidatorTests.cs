using AcademicProjects.Application.Features.Categories.Commands;

namespace AcademicProjects.Tests.Application.Features.Categories;

public class UpdateCategoryCommandValidatorTests
{
    private readonly UpdateCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new UpdateCategoryCommand(Guid.NewGuid(), "Category", "Description"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(
            new UpdateCategoryCommand(Guid.Empty, "Category", "Description"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCategoryCommand.Id));
    }

    [Fact]
    public void Validate_EmptyName_HasError()
    {
        var result = _validator.Validate(
            new UpdateCategoryCommand(Guid.NewGuid(), "", "Description"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCategoryCommand.Name));
    }
}
