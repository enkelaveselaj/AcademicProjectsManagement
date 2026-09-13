using AcademicProjects.Application.Authentication;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Auth.Commands;
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
        RegisterUserCommand request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var personalIdNumber = request.PersonalIdNumber.Trim();
        var studentId = request.RequestedRole == UserRole.Student ? request.StudentId!.Trim() : null;

        // These uniqueness checks need Identity's user data, which isn't visible outside
        // Infrastructure - the rest of the request's validation lives in
        // RegisterUserCommandValidator alongside every other command's FluentValidation rules.
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

    public async Task<AccessToken> LoginAsync(
        LoginCommand request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            throw new InvalidCredentialsException("Invalid email or password.");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            throw new InvalidCredentialsException(
                "This account is temporarily locked due to too many failed sign-in attempts. Please try again later.");
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);

            throw new InvalidCredentialsException("Invalid email or password.");
        }

        await userManager.ResetAccessFailedCountAsync(user);

        if (user.ApprovalStatus != ApprovalStatus.Approved)
        {
            throw new ForbiddenAccessException("Your account is awaiting administrator approval.");
        }

        var roles = await userManager.GetRolesAsync(user);

        return accessTokenGenerator.Generate(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            roles);
    }

    public async Task<ServiceResult<bool>> ChangePasswordAsync(
        ChangePasswordCommand request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return ServiceResult<bool>.Failure(new Dictionary<string, string[]>
            {
                ["password"] = ["Could not change the password for this account."]
            });
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return ServiceResult<bool>.Failure(ToErrors(result));
        }

        return ServiceResult<bool>.Success(true);
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
