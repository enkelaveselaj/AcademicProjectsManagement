using AcademicProjects.Application.Features.Notifications.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Notifications;

public class DeleteNotificationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingNotification_RemovesItAndReturnsTrue()
    {
        using var context = TestDbContextFactory.Create();
        var notification = new Notification
        {
            Message = "Message",
            Type = NotificationType.Information,
            UserId = Guid.NewGuid()
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteNotificationCommandHandler(context);

        var result = await handler.Handle(
            new DeleteNotificationCommand(notification.Id),
            CancellationToken.None);

        Assert.True(result);
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task Handle_NonExistentNotification_ReturnsFalse()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteNotificationCommandHandler(context);

        var result = await handler.Handle(
            new DeleteNotificationCommand(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result);
    }
}
