namespace AcademicProjects.Application.Authentication;

public interface IAuthenticationService
{
    Task<ServiceResult<RegisteredUser>> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AccessToken>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
