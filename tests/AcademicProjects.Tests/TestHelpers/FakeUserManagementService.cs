using AcademicProjects.Application.Authentication;

namespace AcademicProjects.Tests.TestHelpers;

public sealed class FakeUserManagementService : IUserManagementService
{
    private readonly Dictionary<Guid, string> _roles = new();

    public FakeUserManagementService WithUser(Guid userId, string role)
    {
        _roles[userId] = role;
        return this;
    }

    public Task<IReadOnlyList<UserSummary>> GetUsersAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<UserSummary>>([]);

    public Task<ServiceResult<UserSummary>> ChangeUserRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<PendingUser>> GetPendingUsersAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PendingUser>>([]);

    public Task<ServiceResult<UserSummary>> ApproveUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<ServiceResult<bool>> RejectUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<UserDirectoryEntry>> GetUserDirectoryAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<UserDirectoryEntry>>([]);

    public Task<string?> GetUserRoleAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_roles.GetValueOrDefault(userId));
}
