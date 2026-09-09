using AcademicProjects.Application.Authentication;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Infrastructure.Identity;

public sealed class IdentityUserManagementService(
    UserManager<ApplicationUser> userManager) : IUserManagementService
{
    public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users
            .OrderBy(user => user.Email)
            .ToListAsync(cancellationToken);

        var summaries = new List<UserSummary>(users.Count);

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);

            summaries.Add(new UserSummary(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email ?? string.Empty,
                roles.FirstOrDefault() ?? string.Empty));
        }

        return summaries;
    }

    public async Task<ServiceResult<UserSummary>> ChangeUserRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsedRole))
        {
            return ServiceResult<UserSummary>.Failure(new Dictionary<string, string[]>
            {
                ["role"] = [$"'{role}' is not a valid role. Valid roles are: {string.Join(", ", Enum.GetNames<UserRole>())}."]
            });
        }

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        var currentRoles = await userManager.GetRolesAsync(user);

        if (currentRoles.Contains(UserRole.Administrator.ToString()) && parsedRole != UserRole.Administrator)
        {
            var administrators = await userManager.GetUsersInRoleAsync(UserRole.Administrator.ToString());

            if (administrators.Count <= 1)
            {
                return ServiceResult<UserSummary>.Failure(new Dictionary<string, string[]>
                {
                    ["role"] = ["Cannot remove the last administrator."]
                });
            }
        }

        if (currentRoles.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return ServiceResult<UserSummary>.Failure(ToErrors(removeResult));
            }
        }

        var addResult = await userManager.AddToRoleAsync(user, parsedRole.ToString());
        if (!addResult.Succeeded)
        {
            return ServiceResult<UserSummary>.Failure(ToErrors(addResult));
        }

        return ServiceResult<UserSummary>.Success(
            new UserSummary(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, parsedRole.ToString()));
    }

    private static Dictionary<string, string[]> ToErrors(IdentityResult result) =>
        result.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());
}
