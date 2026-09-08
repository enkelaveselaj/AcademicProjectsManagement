using AcademicProjects.Application.Features.Notifications.Commands;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Tests.Application.Features.Notifications;

public class UpdateNotificationCommandValidatorTests
{
    private readonly UpdateNotificationCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new UpdateNotificationCommand(Guid.NewGuid(), "Message", NotificationType.Information, false, Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(
            new UpdateNotificationCommand(Guid.Empty, "Message", NotificationType.Information, false, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateNotificationCommand.Id));
    }
}
