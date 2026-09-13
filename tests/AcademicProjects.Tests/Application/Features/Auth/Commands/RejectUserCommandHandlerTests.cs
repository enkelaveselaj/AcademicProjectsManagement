using AcademicProjects.Application.Features.Auth.Commands;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Auth.Commands;

public class RejectUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToUserManagementServiceAndReturnsItsResult()
    {
        var userManagementService = new FakeUserManagementService();
        var handler = new RejectUserCommandHandler(userManagementService);
        var userId = Guid.NewGuid();

        var result = await handler.Handle(new RejectUserCommand(userId), CancellationToken.None);

        Assert.Same(userManagementService.RejectUserResult, result);
        Assert.Equal(userId, userManagementService.LastRejectUserRequest);
    }
}
