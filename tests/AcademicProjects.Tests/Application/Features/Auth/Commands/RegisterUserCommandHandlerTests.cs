using AcademicProjects.Application.Features.Auth.Commands;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Auth.Commands;

public class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToAuthenticationServiceAndReturnsItsResult()
    {
        var authenticationService = new FakeAuthenticationService();
        var handler = new RegisterUserCommandHandler(authenticationService);
        var command = new RegisterUserCommand(
            "Jane", "Doe", "jane@example.com", "Password1!", new DateTime(2000, 1, 1), "ID123", UserRole.Student, "S123");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Same(authenticationService.RegisterResult, result);
        Assert.Same(command, authenticationService.LastRegisterRequest);
    }
}
