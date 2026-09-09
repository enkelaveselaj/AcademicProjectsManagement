using AcademicProjects.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicProjects.Infrastructure.Identity;

public static class IdentityDataSeeder
{
    public static async Task SeedIdentityRolesAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var roleName in Enum.GetNames<UserRole>())
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Could not create the '{roleName}' role: {errors}");
                }
            }
        }
    }

    /// <summary>
    /// Seeds a single Administrator account from configuration, if one doesn't already exist for
    /// that email. This exists to break the bootstrap problem: registration always creates
    /// Students, so without a seeded admin there would be no way to promote anyone to Mentor or
    /// Administrator. Configure "AdminUser:Email" and "AdminUser:Password" to enable it; leaving
    /// either unset (as in production) skips seeding entirely.
    /// </summary>
    public static async Task SeedAdministratorAsync(
        this IServiceProvider services,
        IConfiguration configuration)
    {
        var email = configuration["AdminUser:Email"];
        var password = configuration["AdminUser:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            FirstName = "Admin",
            LastName = "User",
            UserName = email,
            Email = email
        };

        var createResult = await userManager.CreateAsync(admin, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not create the seeded administrator account: {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(admin, UserRole.Administrator.ToString());
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not assign the administrator role: {errors}");
        }
    }
}
