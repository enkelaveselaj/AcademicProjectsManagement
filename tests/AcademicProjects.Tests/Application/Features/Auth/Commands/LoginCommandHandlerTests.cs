using AcademicProjects.Application.Features.Auth.Commands;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Auth.Commands;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToAuthenticationServiceAndReturnsItsResult()
    {
        var authenticationService = new FakeAuthenticationService();
        var handler = new LoginCommandHandler(authenticationService);
        var command = new LoginCommand("jane@example.com", "Password1!");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Same(authenticationService.LoginResult, result);
        Assert.Same(command, authenticationService.LastLoginRequest);
    }
}
