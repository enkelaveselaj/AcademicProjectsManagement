using AcademicProjects.Application.Features.Auth.Commands;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Tests.Application.Features.Auth.Commands;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    private static RegisterUserCommand ValidStudentCommand() => new(
        "Jane", "Doe", "jane@example.com", "Password1!", DateTime.UtcNow.AddYears(-20), "ID123", UserRole.Student, "S123");

    [Fact]
    public void Validate_ValidStudentCommand_IsValid()
    {
        var result = _validator.Validate(ValidStudentCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ValidMentorCommandWithoutStudentId_IsValid()
    {
        var command = ValidStudentCommand() with { RequestedRole = UserRole.Mentor, StudentId = null };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyFirstName_HasError()
    {
        var result = _validator.Validate(ValidStudentCommand() with { FirstName = "" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.FirstName));
    }

    [Fact]
    public void Validate_AdministratorRole_HasError()
    {
        var result = _validator.Validate(ValidStudentCommand() with { RequestedRole = UserRole.Administrator });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.RequestedRole));
    }

    [Fact]
    public void Validate_StudentWithoutStudentId_HasError()
    {
        var result = _validator.Validate(ValidStudentCommand() with { StudentId = null });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.StudentId));
    }

    [Fact]
    public void Validate_DateOfBirthInFuture_HasError()
    {
        var result = _validator.Validate(ValidStudentCommand() with { DateOfBirth = DateTime.UtcNow.AddDays(1) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.DateOfBirth));
    }

    [Fact]
    public void Validate_YoungerThan16_HasError()
    {
        var result = _validator.Validate(ValidStudentCommand() with { DateOfBirth = DateTime.UtcNow.AddYears(-15) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.DateOfBirth));
    }
}
