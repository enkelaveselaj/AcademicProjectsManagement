using AcademicProjects.Application.Features.Comments.Commands;

namespace AcademicProjects.Tests.Application.Features.Comments;

public class UpdateCommentCommandValidatorTests
{
    private readonly UpdateCommentCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new UpdateCommentCommand(Guid.NewGuid(), "Content", Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyContent_HasError()
    {
        var result = _validator.Validate(
            new UpdateCommentCommand(Guid.NewGuid(), "", Guid.NewGuid()));

        Assert.False(result.IsValid);
    }
}
