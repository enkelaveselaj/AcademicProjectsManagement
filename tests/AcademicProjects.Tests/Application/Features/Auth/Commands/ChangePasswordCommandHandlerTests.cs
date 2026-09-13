using AcademicProjects.Application.Features.Auth.Commands;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Auth.Commands;

public class ChangePasswordCommandHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToAuthenticationServiceAndReturnsItsResult()
    {
        var authenticationService = new FakeAuthenticationService();
        var handler = new ChangePasswordCommandHandler(authenticationService);
        var command = new ChangePasswordCommand(Guid.NewGuid(), "OldPassword1!", "NewPassword1!");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Same(authenticationService.ChangePasswordResult, result);
        Assert.Same(command, authenticationService.LastChangePasswordRequest);
    }
}
