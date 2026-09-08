using AcademicProjects.Application.Features.Comments.Commands;

namespace AcademicProjects.Tests.Application.Features.Comments;

public class CreateCommentCommandValidatorTests
{
    private readonly CreateCommentCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(new CreateCommentCommand("Content", Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyContent_HasError()
    {
        var result = _validator.Validate(new CreateCommentCommand("", Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCommentCommand.Content));
    }

    [Fact]
    public void Validate_ContentTooLong_HasError()
    {
        var result = _validator.Validate(new CreateCommentCommand(new string('a', 2001), Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCommentCommand.Content));
    }

    [Fact]
    public void Validate_EmptyProjectId_HasError()
    {
        var result = _validator.Validate(new CreateCommentCommand("Content", Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCommentCommand.ProjectId));
    }
}
