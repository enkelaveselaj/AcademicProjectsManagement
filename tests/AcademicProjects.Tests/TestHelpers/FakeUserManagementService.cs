using AcademicProjects.Application.Authentication;

namespace AcademicProjects.Tests.TestHelpers;

/// <summary>
/// Configurable in-memory stand-in for IUserManagementService. Records the last arguments it
/// received per method, so handler tests can verify delegation without a real Identity/EF Core
/// setup, and exposes settable *Result properties for each read method.
/// </summary>
public sealed class FakeUserManagementService : IUserManagementService
{
    private readonly Dictionary<Guid, string> _roles = new();

    public (Guid UserId, string Role)? LastChangeUserRoleRequest { get; private set; }
    public Guid? LastApproveUserRequest { get; private set; }
    public Guid? LastRejectUserRequest { get; private set; }
    public (Guid UserId, string NewPassword)? LastResetUserPasswordRequest { get; private set; }

    public IReadOnlyList<UserSummary> UsersResult { get; set; } = [];
    public IReadOnlyList<PendingUser> PendingUsersResult { get; set; } = [];
    public IReadOnlyList<UserDirectoryEntry> DirectoryResult { get; set; } = [];
    public ServiceResult<UserSummary> ChangeUserRoleResult { get; set; } =
        ServiceResult<UserSummary>.Success(new UserSummary(Guid.NewGuid(), "First", "Last", "user@example.com", "Student"));
    public ServiceResult<UserSummary> ApproveUserResult { get; set; } =
        ServiceResult<UserSummary>.Success(new UserSummary(Guid.NewGuid(), "First", "Last", "user@example.com", "Student"));
    public ServiceResult<bool> RejectUserResult { get; set; } = ServiceResult<bool>.Success(true);
    public ServiceResult<bool> ResetUserPasswordResult { get; set; } = ServiceResult<bool>.Success(true);

    public FakeUserManagementService WithUser(Guid userId, string role)
    {
        _roles[userId] = role;
        return this;
    }

    public Task<IReadOnlyList<UserSummary>> GetUsersAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(UsersResult);

    public Task<ServiceResult<UserSummary>> ChangeUserRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        LastChangeUserRoleRequest = (userId, role);
        return Task.FromResult(ChangeUserRoleResult);
    }

    public Task<IReadOnlyList<PendingUser>> GetPendingUsersAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(PendingUsersResult);

    public Task<ServiceResult<UserSummary>> ApproveUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        LastApproveUserRequest = userId;
        return Task.FromResult(ApproveUserResult);
    }

    public Task<ServiceResult<bool>> RejectUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        LastRejectUserRequest = userId;
        return Task.FromResult(RejectUserResult);
    }

    public Task<IReadOnlyList<UserDirectoryEntry>> GetUserDirectoryAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(DirectoryResult);

    public Task<string?> GetUserRoleAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_roles.GetValueOrDefault(userId));

    public Task<ServiceResult<bool>> ResetUserPasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        LastResetUserPasswordRequest = (userId, newPassword);
        return Task.FromResult(ResetUserPasswordResult);
    }
}
