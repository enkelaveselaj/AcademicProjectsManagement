using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Notifications.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Notifications;

public class UpdateNotificationCommandHandlerTests
{
    [Fact]
    public async Task Handle_Recipient_UpdatesAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var recipient = TestCurrentUserService.AsStudent();
        var notification = new Notification
        {
            Message = "Old",
            Type = NotificationType.Information,
            IsRead = false,
            UserId = recipient.UserId!.Value
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateNotificationCommandHandler(context, recipient);

        var result = await handler.Handle(
            new UpdateNotificationCommand(notification.Id, "New message", NotificationType.Success, true, recipient.UserId!.Value),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New message", result!.Message);
        Assert.Equal(NotificationType.Success, result.Type);
        Assert.True(result.IsRead);
    }

    [Fact]
    public async Task Handle_NonExistentNotification_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdateNotificationCommandHandler(context, TestCurrentUserService.AsAdministrator());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateNotificationCommand(Guid.NewGuid(), "Message", NotificationType.Error, false, Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonRecipient_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var recipient = TestCurrentUserService.AsStudent();
        var notification = new Notification
        {
            Message = "Old",
            Type = NotificationType.Information,
            UserId = recipient.UserId!.Value
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);

        var otherStudent = TestCurrentUserService.AsStudent();
        var handler = new UpdateNotificationCommandHandler(context, otherStudent);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new UpdateNotificationCommand(notification.Id, "Hijacked", NotificationType.Error, true, recipient.UserId!.Value),
                CancellationToken.None));
    }
}
