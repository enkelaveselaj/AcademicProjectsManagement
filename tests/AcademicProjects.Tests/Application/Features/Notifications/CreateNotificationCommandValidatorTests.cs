using AcademicProjects.Application.Features.Notifications.Commands;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Tests.Application.Features.Notifications;

public class CreateNotificationCommandValidatorTests
{
    private readonly CreateNotificationCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new CreateNotificationCommand("Message", NotificationType.Information, false, Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyMessage_HasError()
    {
        var result = _validator.Validate(
            new CreateNotificationCommand("", NotificationType.Information, false, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateNotificationCommand.Message));
    }

    [Fact]
    public void Validate_InvalidType_HasError()
    {
        var result = _validator.Validate(
            new CreateNotificationCommand("Message", (NotificationType)999, false, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateNotificationCommand.Type));
    }

    [Fact]
    public void Validate_EmptyUserId_HasError()
    {
        var result = _validator.Validate(
            new CreateNotificationCommand("Message", NotificationType.Information, false, Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateNotificationCommand.UserId));
    }
}
