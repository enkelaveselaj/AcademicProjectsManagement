using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Notifications.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Notifications;

public class DeleteNotificationCommandHandlerTests
{
    [Fact]
    public async Task Handle_Recipient_RemovesIt()
    {
        using var context = TestDbContextFactory.Create();
        var recipient = TestCurrentUserService.AsStudent();
        var notification = new Notification
        {
            Message = "Message",
            Type = NotificationType.Information,
            UserId = recipient.UserId!.Value
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteNotificationCommandHandler(context, recipient);

        await handler.Handle(
            new DeleteNotificationCommand(notification.Id),
            CancellationToken.None);

        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task Handle_NonExistentNotification_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteNotificationCommandHandler(context, TestCurrentUserService.AsAdministrator());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new DeleteNotificationCommand(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonRecipient_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var recipient = TestCurrentUserService.AsStudent();
        var notification = new Notification
        {
            Message = "Message",
            Type = NotificationType.Information,
            UserId = recipient.UserId!.Value
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);

        var otherStudent = TestCurrentUserService.AsStudent();
        var handler = new DeleteNotificationCommandHandler(context, otherStudent);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new DeleteNotificationCommand(notification.Id),
                CancellationToken.None));
    }
}
