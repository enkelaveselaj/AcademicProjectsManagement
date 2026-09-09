using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Notifications.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Notifications;

public class GetNotificationByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_Recipient_ReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var recipient = TestCurrentUserService.AsStudent();
        var notification = new Notification { Message = "Message", Type = NotificationType.Information, UserId = recipient.UserId!.Value };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetNotificationByIdQueryHandler(context, recipient);

        var result = await handler.Handle(
            new GetNotificationByIdQuery(notification.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(notification.Id, result!.Id);
    }

    [Fact]
    public async Task Handle_NonExistentNotification_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetNotificationByIdQueryHandler(context, TestCurrentUserService.AsAdministrator());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new GetNotificationByIdQuery(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonRecipient_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var notification = new Notification { Message = "Message", Type = NotificationType.Information, UserId = Guid.NewGuid() };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetNotificationByIdQueryHandler(context, TestCurrentUserService.AsStudent());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new GetNotificationByIdQuery(notification.Id),
                CancellationToken.None));
    }
}
