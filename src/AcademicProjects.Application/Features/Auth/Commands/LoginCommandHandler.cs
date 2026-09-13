using AcademicProjects.Application.Authentication;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed class LoginCommandHandler(IAuthenticationService authenticationService)
    : IRequestHandler<LoginCommand, AccessToken>
{
    public Task<AccessToken> Handle(
        LoginCommand request,
        CancellationToken cancellationToken) =>
        authenticationService.LoginAsync(request, cancellationToken);
}
