using AcademicProjects.Application.Features.Auth.Commands;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Auth.Commands;

public class ChangeUserRoleCommandHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToUserManagementServiceAndReturnsItsResult()
    {
        var userManagementService = new FakeUserManagementService();
        var handler = new ChangeUserRoleCommandHandler(userManagementService);
        var userId = Guid.NewGuid();

        var result = await handler.Handle(new ChangeUserRoleCommand(userId, "Mentor"), CancellationToken.None);

        Assert.Same(userManagementService.ChangeUserRoleResult, result);
        Assert.Equal((userId, "Mentor"), userManagementService.LastChangeUserRoleRequest);
    }
}
