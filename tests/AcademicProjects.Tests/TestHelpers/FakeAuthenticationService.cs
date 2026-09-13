using AcademicProjects.Application.Authentication;
using AcademicProjects.Application.Features.Auth.Commands;

namespace AcademicProjects.Tests.TestHelpers;

/// <summary>
/// Records the last request it received and returns whatever result was configured, so handler
/// tests can verify the handler delegates to IAuthenticationService without needing a real
/// Identity/EF Core setup.
/// </summary>
public sealed class FakeAuthenticationService : IAuthenticationService
{
    public RegisterUserCommand? LastRegisterRequest { get; private set; }
    public LoginCommand? LastLoginRequest { get; private set; }
    public ChangePasswordCommand? LastChangePasswordRequest { get; private set; }

    public ServiceResult<RegisteredUser> RegisterResult { get; set; } =
        ServiceResult<RegisteredUser>.Success(new RegisteredUser(Guid.NewGuid(), "user@example.com", "Student", "Pending"));

    public AccessToken LoginResult { get; set; } = new("token", 3600);

    public ServiceResult<bool> ChangePasswordResult { get; set; } = ServiceResult<bool>.Success(true);

    public Task<ServiceResult<RegisteredUser>> RegisterAsync(
        RegisterUserCommand request,
        CancellationToken cancellationToken = default)
    {
        LastRegisterRequest = request;
        return Task.FromResult(RegisterResult);
    }

    public Task<AccessToken> LoginAsync(
        LoginCommand request,
        CancellationToken cancellationToken = default)
    {
        LastLoginRequest = request;
        return Task.FromResult(LoginResult);
    }

    public Task<ServiceResult<bool>> ChangePasswordAsync(
        ChangePasswordCommand request,
        CancellationToken cancellationToken = default)
    {
        LastChangePasswordRequest = request;
        return Task.FromResult(ChangePasswordResult);
    }
}
