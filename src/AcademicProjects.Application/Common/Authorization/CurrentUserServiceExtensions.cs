using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Application.Common.Authorization;

public static class CurrentUserServiceExtensions
{
    public static Guid GetUserId(this ICurrentUserService currentUser) =>
        currentUser.UserId
            ?? throw new InvalidOperationException("The current request is not authenticated.");

    public static bool IsInRole(this ICurrentUserService currentUser, UserRole role) =>
        currentUser.Roles.Contains(role.ToString());

    public static bool IsAdministrator(this ICurrentUserService currentUser) =>
        currentUser.IsInRole(UserRole.Administrator);
}
