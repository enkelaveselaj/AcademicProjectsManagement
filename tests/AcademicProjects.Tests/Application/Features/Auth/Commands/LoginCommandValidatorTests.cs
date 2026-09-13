using AcademicProjects.Application.Features.Auth.Commands;

namespace AcademicProjects.Tests.Application.Features.Auth.Commands;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(new LoginCommand("jane@example.com", "Password1!"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyEmail_HasError()
    {
        var result = _validator.Validate(new LoginCommand("", "Password1!"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public void Validate_EmptyPassword_HasError()
    {
        var result = _validator.Validate(new LoginCommand("jane@example.com", ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginCommand.Password));
    }
}
