using AcademicProjects.Application.Authentication;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Infrastructure.Identity;

public sealed class IdentityAuthenticationService(
    UserManager<ApplicationUser> userManager,
    IAccessTokenGenerator accessTokenGenerator,
    IApplicationDbContext context) : IAuthenticationService
{
    public async Task<ServiceResult<RegisteredUser>> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateRegistration(request);
        if (errors.Count > 0)
        {
            return ServiceResult<RegisteredUser>.Failure(errors);
        }

        var email = request.Email.Trim();
        var personalIdNumber = request.PersonalIdNumber.Trim();
        var studentId = request.RequestedRole == UserRole.Student ? request.StudentId!.Trim() : null;

        if (await userManager.Users.AnyAsync(user => user.PersonalIdNumber == personalIdNumber, cancellationToken))
        {
            return ServiceResult<RegisteredUser>.Failure(new Dictionary<string, string[]>
            {
                ["personalIdNumber"] = ["An account with this ID number already exists."]
            });
        }

        if (studentId is not null
            && await userManager.Users.AnyAsync(user => user.StudentId == studentId, cancellationToken))
        {
            return ServiceResult<RegisteredUser>.Failure(new Dictionary<string, string[]>
            {
                ["studentId"] = ["An account with this student ID already exists."]
            });
        }

        var user = new ApplicationUser
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            UserName = email,
            Email = email,
            DateOfBirth = request.DateOfBirth.Date,
            PersonalIdNumber = personalIdNumber,
            StudentId = studentId,
            ApprovalStatus = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return ServiceResult<RegisteredUser>.Failure(ToErrors(createResult));
        }

        var roleResult = await userManager.AddToRoleAsync(user, request.RequestedRole.ToString());
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return ServiceResult<RegisteredUser>.Failure(ToErrors(roleResult));
        }

        await NotifyAdministratorsOfPendingRequestAsync(user, request.RequestedRole, cancellationToken);

        return ServiceResult<RegisteredUser>.Success(
            new RegisteredUser(
                user.Id,
                user.Email!,
                request.RequestedRole.ToString(),
                user.ApprovalStatus.ToString()));
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

        if (user.ApprovalStatus != ApprovalStatus.Approved)
        {
            return ServiceResult<AccessToken>.Failure(new Dictionary<string, string[]>
            {
                ["approval"] = ["Your account is awaiting administrator approval."]
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

    private async Task NotifyAdministratorsOfPendingRequestAsync(
        ApplicationUser user,
        UserRole requestedRole,
        CancellationToken cancellationToken)
    {
        var administrators = await userManager.GetUsersInRoleAsync(UserRole.Administrator.ToString());

        if (administrators.Count == 0)
        {
            return;
        }

        var message = $"{user.FirstName} {user.LastName} requested a {requestedRole} account.";

        foreach (var administrator in administrators)
        {
            context.Notifications.Add(new Notification
            {
                Message = message,
                Type = NotificationType.Information,
                IsRead = false,
                UserId = administrator.Id
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static Dictionary<string, string[]> ValidateRegistration(RegisterUserRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            errors["firstName"] = ["First name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            errors["lastName"] = ["Last name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = ["Email is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors["password"] = ["Password is required."];
        }

        if (string.IsNullOrWhiteSpace(request.PersonalIdNumber))
        {
            errors["personalIdNumber"] = ["A personal ID number is required."];
        }

        if (request.RequestedRole is not (UserRole.Student or UserRole.Mentor))
        {
            errors["requestedRole"] = ["You can only request a Student or Mentor account."];
        }
        else if (request.RequestedRole == UserRole.Student && string.IsNullOrWhiteSpace(request.StudentId))
        {
            errors["studentId"] = ["A student ID is required for a Student account."];
        }

        var today = DateTime.UtcNow.Date;
        if (request.DateOfBirth.Date > today)
        {
            errors["dateOfBirth"] = ["Date of birth cannot be in the future."];
        }
        else if (request.DateOfBirth.Date > today.AddYears(-16))
        {
            errors["dateOfBirth"] = ["You must be at least 16 years old to register."];
        }

        return errors;
    }

    /// <summary>
    /// Buckets Identity's own error codes (e.g. "PasswordRequiresDigit", "DuplicateEmail") under the
    /// same field-name keys used elsewhere in this method's validation, so the client can always look
    /// up an error by field regardless of whether it came from our checks or from Identity's.
    /// </summary>
    private static Dictionary<string, string[]> ToErrors(IdentityResult result) =>
        result.Errors
            .GroupBy(error => error.Code.StartsWith("Password", StringComparison.Ordinal)
                ? "password"
                : error.Code is "DuplicateUserName" or "DuplicateEmail" or "InvalidUserName" or "InvalidEmail"
                    ? "email"
                    : "general")
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());
}
