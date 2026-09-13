using AcademicProjects.Application.Authentication;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Infrastructure.Identity;

public sealed class IdentityUserManagementService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext identityContext,
    IApplicationDbContext context) : IUserManagementService
{
    /// <summary>
    /// Loads users (optionally filtered) together with their role name in a single query,
    /// instead of one extra round-trip per user via UserManager.GetRolesAsync.
    /// </summary>
    private async Task<List<(ApplicationUser User, string RoleName)>> GetUsersWithRolesAsync(
        IQueryable<ApplicationUser> usersQuery,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from user in usersQuery
            join userRole in identityContext.UserRoles on user.Id equals userRole.UserId into userRoles
            from userRole in userRoles.DefaultIfEmpty()
            join role in identityContext.Roles on userRole.RoleId equals role.Id into roles
            from role in roles.DefaultIfEmpty()
            select new { User = user, RoleName = role.Name })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.User.Id)
            .Select(group => (
                group.First().User,
                group.Select(row => row.RoleName).FirstOrDefault(name => name is not null) ?? string.Empty))
            .ToList();
    }

    public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(
        CancellationToken cancellationToken = default)
    {
        var usersWithRoles = await GetUsersWithRolesAsync(userManager.Users, cancellationToken);

        return usersWithRoles
            .OrderBy(x => x.User.Email)
            .Select(x => new UserSummary(x.User.Id, x.User.FirstName, x.User.LastName, x.User.Email ?? string.Empty, x.RoleName))
            .ToList();
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

    public async Task<IReadOnlyList<PendingUser>> GetPendingUsersAsync(
        CancellationToken cancellationToken = default)
    {
        var usersWithRoles = await GetUsersWithRolesAsync(
            userManager.Users.Where(user => user.ApprovalStatus == ApprovalStatus.Pending),
            cancellationToken);

        return usersWithRoles
            .OrderBy(x => x.User.CreatedAt)
            .Select(x => new PendingUser(
                x.User.Id,
                x.User.FirstName,
                x.User.LastName,
                x.User.Email ?? string.Empty,
                x.RoleName,
                x.User.DateOfBirth,
                x.User.PersonalIdNumber,
                x.User.StudentId,
                x.User.CreatedAt))
            .ToList();
    }

    public async Task<ServiceResult<UserSummary>> ApproveUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        user.ApprovalStatus = ApprovalStatus.Approved;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return ServiceResult<UserSummary>.Failure(ToErrors(updateResult));
        }

        var roles = await userManager.GetRolesAsync(user);

        return ServiceResult<UserSummary>.Success(
            new UserSummary(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, roles.FirstOrDefault() ?? string.Empty));
    }

    public async Task<ServiceResult<bool>> RejectUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        if (user.ApprovalStatus != ApprovalStatus.Pending)
        {
            return ServiceResult<bool>.Failure(new Dictionary<string, string[]>
            {
                ["approvalStatus"] = ["Only a pending registration can be rejected."]
            });
        }

        await userManager.DeleteAsync(user);

        return ServiceResult<bool>.Success(true);
    }

    public async Task<IReadOnlyList<UserDirectoryEntry>> GetUserDirectoryAsync(
        CancellationToken cancellationToken = default)
    {
        var usersWithRoles = await GetUsersWithRolesAsync(
            userManager.Users.Where(user => user.ApprovalStatus == ApprovalStatus.Approved),
            cancellationToken);

        return usersWithRoles
            .OrderBy(x => x.User.FirstName)
            .Select(x => new UserDirectoryEntry(x.User.Id, x.User.FirstName, x.User.LastName, x.RoleName))
            .ToList();
    }

    public async Task<string?> GetUserRoleAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);

        return roles.FirstOrDefault();
    }

    public async Task<ServiceResult<bool>> ResetUserPasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return ServiceResult<bool>.Failure(new Dictionary<string, string[]>
            {
                ["password"] = ["A new password is required."]
            });
        }

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, newPassword);

        if (!result.Succeeded)
        {
            return ServiceResult<bool>.Failure(new Dictionary<string, string[]>
            {
                ["password"] = result.Errors.Select(error => error.Description).ToArray()
            });
        }

        context.Notifications.Add(new Notification
        {
            Message = "Your password was reset by an administrator.",
            Type = NotificationType.Warning,
            UserId = user.Id
        });

        await context.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }

    private static Dictionary<string, string[]> ToErrors(IdentityResult result) =>
        result.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());
}
