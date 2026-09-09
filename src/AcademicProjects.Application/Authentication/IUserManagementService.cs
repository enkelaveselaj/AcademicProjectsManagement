namespace AcademicProjects.Application.Authentication;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserSummary>> GetUsersAsync(
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserSummary>> ChangeUserRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default);
}
