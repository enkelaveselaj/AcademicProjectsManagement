namespace AcademicProjects.Application.Authentication;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserSummary>> GetUsersAsync(
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserSummary>> ChangeUserRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PendingUser>> GetPendingUsersAsync(
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserSummary>> ApproveUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> RejectUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDirectoryEntry>> GetUserDirectoryAsync(
        CancellationToken cancellationToken = default);
}
