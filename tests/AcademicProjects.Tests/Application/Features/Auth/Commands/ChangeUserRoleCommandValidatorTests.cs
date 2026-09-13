using AcademicProjects.Application.Features.Auth.Commands;

namespace AcademicProjects.Tests.Application.Features.Auth.Commands;

public class ChangeUserRoleCommandValidatorTests
{
    private readonly ChangeUserRoleCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(new ChangeUserRoleCommand(Guid.NewGuid(), "Mentor"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyRole_HasError()
    {
        var result = _validator.Validate(new ChangeUserRoleCommand(Guid.NewGuid(), ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangeUserRoleCommand.Role));
    }
}
