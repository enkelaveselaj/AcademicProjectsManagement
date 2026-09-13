using AcademicProjects.Application.Authentication;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed class RegisterUserCommandHandler(IAuthenticationService authenticationService)
    : IRequestHandler<RegisterUserCommand, ServiceResult<RegisteredUser>>
{
    public Task<ServiceResult<RegisteredUser>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken) =>
        authenticationService.RegisterAsync(request, cancellationToken);
}
