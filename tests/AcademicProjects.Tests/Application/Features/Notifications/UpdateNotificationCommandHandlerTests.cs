using AcademicProjects.Application.Features.Notifications.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Notifications;

public class UpdateNotificationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingNotification_UpdatesAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var notification = new Notification
        {
            Message = "Old",
            Type = NotificationType.Information,
            IsRead = false,
            UserId = Guid.NewGuid()
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateNotificationCommandHandler(context);
        var newUserId = Guid.NewGuid();

        var result = await handler.Handle(
            new UpdateNotificationCommand(notification.Id, "New message", NotificationType.Success, true, newUserId),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New message", result!.Message);
        Assert.Equal(NotificationType.Success, result.Type);
        Assert.True(result.IsRead);
        Assert.Equal(newUserId, result.UserId);
    }

    [Fact]
    public async Task Handle_NonExistentNotification_ReturnsNull()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdateNotificationCommandHandler(context);

        var result = await handler.Handle(
            new UpdateNotificationCommand(Guid.NewGuid(), "Message", NotificationType.Error, false, Guid.NewGuid()),
            CancellationToken.None);

        Assert.Null(result);
    }
}
