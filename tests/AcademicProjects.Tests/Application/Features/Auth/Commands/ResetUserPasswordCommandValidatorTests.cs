using AcademicProjects.Application.Features.Auth.Commands;

namespace AcademicProjects.Tests.Application.Features.Auth.Commands;

public class ResetUserPasswordCommandValidatorTests
{
    private readonly ResetUserPasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(new ResetUserPasswordCommand(Guid.NewGuid(), "NewPassword1!"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyNewPassword_HasError()
    {
        var result = _validator.Validate(new ResetUserPasswordCommand(Guid.NewGuid(), ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetUserPasswordCommand.NewPassword));
    }
}
