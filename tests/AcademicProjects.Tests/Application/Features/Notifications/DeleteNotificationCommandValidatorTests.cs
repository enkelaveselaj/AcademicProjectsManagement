using AcademicProjects.Application.Features.Notifications.Commands;

namespace AcademicProjects.Tests.Application.Features.Notifications;

public class DeleteNotificationCommandValidatorTests
{
    private readonly DeleteNotificationCommandValidator _validator = new();

    [Fact]
    public void Validate_NonEmptyId_IsValid()
    {
        var result = _validator.Validate(new DeleteNotificationCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(new DeleteNotificationCommand(Guid.Empty));

        Assert.False(result.IsValid);
    }
}
