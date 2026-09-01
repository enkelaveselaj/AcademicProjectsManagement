using AcademicProjects.Application.Authentication;
using AcademicProjects.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace AcademicProjects.Infrastructure.Identity;

public sealed class IdentityAuthenticationService(
    UserManager<ApplicationUser> userManager,
    IAccessTokenGenerator accessTokenGenerator) : IAuthenticationService
{
    public async Task<ServiceResult<RegisteredUser>> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName)
            || string.IsNullOrWhiteSpace(request.LastName)
            || string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Password))
        {
            return ServiceResult<RegisteredUser>.Failure(new Dictionary<string, string[]>
            {
                ["user"] = ["First name, last name, email, and password are required."]
            });
        }

        var user = new ApplicationUser
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            UserName = request.Email.Trim(),
            Email = request.Email.Trim()
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return ServiceResult<RegisteredUser>.Failure(ToErrors(createResult));
        }

        var roleResult = await userManager.AddToRoleAsync(user, UserRole.Student.ToString());
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return ServiceResult<RegisteredUser>.Failure(ToErrors(roleResult));
        }

        return ServiceResult<RegisteredUser>.Success(
            new RegisteredUser(user.Id, user.Email!, UserRole.Student.ToString()));
    }

    public async Task<ServiceResult<AccessToken>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ServiceResult<AccessToken>.Failure(new Dictionary<string, string[]>
            {
                ["credentials"] = ["Invalid email or password."]
            });
        }

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return ServiceResult<AccessToken>.Failure(new Dictionary<string, string[]>
            {
                ["credentials"] = ["Invalid email or password."]
            });
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = accessTokenGenerator.Generate(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            roles);

        return ServiceResult<AccessToken>.Success(token);
    }

    private static Dictionary<string, string[]> ToErrors(IdentityResult result) =>
        result.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());
}
