using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Application.Authentication;

public sealed record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    DateTime DateOfBirth,
    string PersonalIdNumber,
    UserRole RequestedRole,
    string? StudentId);

public sealed record LoginRequest(string Email, string Password);

public sealed record RegisteredUser(Guid Id, string Email, string Role, string ApprovalStatus);

public sealed record AccessToken(string Value, int ExpiresInSeconds);

public sealed record UserSummary(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role);

public sealed record PendingUser(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string RequestedRole,
    DateTime DateOfBirth,
    string PersonalIdNumber,
    string? StudentId,
    DateTime RequestedAt);

public sealed record ChangeUserRoleRequest(string Role);

public sealed record ServiceResult<T>(T? Value, IReadOnlyDictionary<string, string[]> Errors)
{
    public bool Succeeded => Errors.Count == 0;

    public static ServiceResult<T> Success(T value) => new(value, EmptyErrors);

    public static ServiceResult<T> Failure(IReadOnlyDictionary<string, string[]> errors) => new(default, errors);

    private static readonly IReadOnlyDictionary<string, string[]> EmptyErrors =
        new Dictionary<string, string[]>();
}
