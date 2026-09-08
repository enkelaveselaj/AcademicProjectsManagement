using AcademicProjects.Application.Features.Notifications.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Notifications;

public class GetNotificationByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ExistingNotification_ReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var notification = new Notification { Message = "Message", Type = NotificationType.Information, UserId = Guid.NewGuid() };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetNotificationByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetNotificationByIdQuery(notification.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(notification.Id, result!.Id);
    }

    [Fact]
    public async Task Handle_NonExistentNotification_ReturnsNull()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetNotificationByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetNotificationByIdQuery(Guid.NewGuid()),
            CancellationToken.None);

        Assert.Null(result);
    }
}
