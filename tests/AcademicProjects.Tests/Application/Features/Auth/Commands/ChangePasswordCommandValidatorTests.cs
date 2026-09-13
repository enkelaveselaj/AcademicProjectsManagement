using AcademicProjects.Application.Features.Auth.Commands;

namespace AcademicProjects.Tests.Application.Features.Auth.Commands;

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(new ChangePasswordCommand(Guid.NewGuid(), "OldPassword1!", "NewPassword1!"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyCurrentPassword_HasError()
    {
        var result = _validator.Validate(new ChangePasswordCommand(Guid.NewGuid(), "", "NewPassword1!"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangePasswordCommand.CurrentPassword));
    }

    [Fact]
    public void Validate_EmptyNewPassword_HasError()
    {
        var result = _validator.Validate(new ChangePasswordCommand(Guid.NewGuid(), "OldPassword1!", ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }
}
