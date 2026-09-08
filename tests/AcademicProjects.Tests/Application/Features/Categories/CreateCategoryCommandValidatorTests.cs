using AcademicProjects.Application.Features.Categories.Commands;

namespace AcademicProjects.Tests.Application.Features.Categories;

public class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(new CreateCategoryCommand("Category", "Description"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyName_HasError()
    {
        var result = _validator.Validate(new CreateCategoryCommand("", "Description"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.Name));
    }

    [Fact]
    public void Validate_NameTooLong_HasError()
    {
        var result = _validator.Validate(new CreateCategoryCommand(new string('a', 121), null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.Name));
    }

    [Fact]
    public void Validate_DescriptionTooLong_HasError()
    {
        var result = _validator.Validate(new CreateCategoryCommand("Category", new string('a', 501)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.Description));
    }
}
