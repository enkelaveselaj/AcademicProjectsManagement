using AcademicProjects.Application.Features.ProjectInvitations.Commands;

namespace AcademicProjects.Tests.Application.Features.ProjectInvitations;

public class CreateProjectInvitationCommandValidatorTests
{
    private readonly CreateProjectInvitationCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new CreateProjectInvitationCommand(Guid.NewGuid(), Guid.NewGuid(), "Student"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_MentorRole_IsValid()
    {
        var result = _validator.Validate(
            new CreateProjectInvitationCommand(Guid.NewGuid(), Guid.NewGuid(), "Mentor"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyProjectId_HasError()
    {
        var result = _validator.Validate(
            new CreateProjectInvitationCommand(Guid.Empty, Guid.NewGuid(), "Student"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectInvitationCommand.ProjectId));
    }

    [Fact]
    public void Validate_EmptyInvitedUserId_HasError()
    {
        var result = _validator.Validate(
            new CreateProjectInvitationCommand(Guid.NewGuid(), Guid.Empty, "Student"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectInvitationCommand.InvitedUserId));
    }

    [Fact]
    public void Validate_DisallowedRole_HasError()
    {
        var result = _validator.Validate(
            new CreateProjectInvitationCommand(Guid.NewGuid(), Guid.NewGuid(), "Administrator"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProjectInvitationCommand.Role));
    }
}
