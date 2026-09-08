using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Notifications.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Notifications;

public class DeleteNotificationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingNotification_RemovesIt()
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

        await handler.Handle(
            new DeleteNotificationCommand(notification.Id),
            CancellationToken.None);

        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task Handle_NonExistentNotification_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteNotificationCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new DeleteNotificationCommand(Guid.NewGuid()),
                CancellationToken.None));
    }
}
