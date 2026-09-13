using AcademicProjects.Application.Features.Auth.Commands;

namespace AcademicProjects.Application.Authentication;

public interface IAuthenticationService
{
    Task<ServiceResult<RegisteredUser>> RegisterAsync(
        RegisterUserCommand request,
        CancellationToken cancellationToken = default);

    Task<AccessToken> LoginAsync(
        LoginCommand request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> ChangePasswordAsync(
        ChangePasswordCommand request,
        CancellationToken cancellationToken = default);
}
