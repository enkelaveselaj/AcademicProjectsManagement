using AcademicProjects.Application.Features.Auth.Commands;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Auth.Commands;

public class ResetUserPasswordCommandHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToUserManagementServiceAndReturnsItsResult()
    {
        var userManagementService = new FakeUserManagementService();
        var handler = new ResetUserPasswordCommandHandler(userManagementService);
        var userId = Guid.NewGuid();

        var result = await handler.Handle(new ResetUserPasswordCommand(userId, "NewPassword1!"), CancellationToken.None);

        Assert.Same(userManagementService.ResetUserPasswordResult, result);
        Assert.Equal((userId, "NewPassword1!"), userManagementService.LastResetUserPasswordRequest);
    }
}
