using AcademicProjects.Application.Features.Auth.Commands;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Auth.Commands;

public class ApproveUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToUserManagementServiceAndReturnsItsResult()
    {
        var userManagementService = new FakeUserManagementService();
        var handler = new ApproveUserCommandHandler(userManagementService);
        var userId = Guid.NewGuid();

        var result = await handler.Handle(new ApproveUserCommand(userId), CancellationToken.None);

        Assert.Same(userManagementService.ApproveUserResult, result);
        Assert.Equal(userId, userManagementService.LastApproveUserRequest);
    }
}
