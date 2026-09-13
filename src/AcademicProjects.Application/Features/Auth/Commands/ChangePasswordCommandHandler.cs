using AcademicProjects.Application.Authentication;
using MediatR;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed class ChangePasswordCommandHandler(IAuthenticationService authenticationService)
    : IRequestHandler<ChangePasswordCommand, ServiceResult<bool>>
{
    public Task<ServiceResult<bool>> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken) =>
        authenticationService.ChangePasswordAsync(request, cancellationToken);
}
