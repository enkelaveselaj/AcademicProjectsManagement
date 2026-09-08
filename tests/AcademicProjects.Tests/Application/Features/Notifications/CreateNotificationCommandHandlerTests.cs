using AcademicProjects.Application.Features.Notifications.Commands;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Notifications;

public class CreateNotificationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesNotificationAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateNotificationCommandHandler(context);
        var userId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateNotificationCommand("Deadline approaching", NotificationType.Warning, false, userId),
            CancellationToken.None);

        Assert.Equal("Deadline approaching", result.Message);
        Assert.Equal(NotificationType.Warning, result.Type);
        Assert.False(result.IsRead);
        Assert.Equal(userId, result.UserId);
        Assert.Single(context.Notifications);
    }
}
