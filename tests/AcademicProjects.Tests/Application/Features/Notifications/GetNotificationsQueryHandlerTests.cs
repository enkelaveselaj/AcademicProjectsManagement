using AcademicProjects.Application.Features.Notifications.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Notifications;

public class GetNotificationsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Administrator_ReturnsAllNotifications()
    {
        using var context = TestDbContextFactory.Create();
        context.Notifications.AddRange(
            new Notification { Message = "A", Type = NotificationType.Information, UserId = Guid.NewGuid() },
            new Notification { Message = "B", Type = NotificationType.Warning, UserId = Guid.NewGuid() });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetNotificationsQueryHandler(context, TestCurrentUserService.AsAdministrator());

        var result = await handler.Handle(new GetNotificationsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_NoNotifications_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetNotificationsQueryHandler(context, TestCurrentUserService.AsAdministrator());

        var result = await handler.Handle(new GetNotificationsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_Student_OnlyReturnsOwnNotifications()
    {
        using var context = TestDbContextFactory.Create();
        var student = TestCurrentUserService.AsStudent();
        context.Notifications.AddRange(
            new Notification { Message = "Mine", Type = NotificationType.Information, UserId = student.UserId!.Value },
            new Notification { Message = "Someone else's", Type = NotificationType.Information, UserId = Guid.NewGuid() });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetNotificationsQueryHandler(context, student);

        var result = await handler.Handle(new GetNotificationsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Mine", result[0].Message);
    }
}
