namespace AcademicProjects.Application.Authentication;

public sealed record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record RegisteredUser(Guid Id, string Email, string Role);

public sealed record AccessToken(string Value, int ExpiresInSeconds);

public sealed record ServiceResult<T>(T? Value, IReadOnlyDictionary<string, string[]> Errors)
    where T : class
{
    public bool Succeeded => Errors.Count == 0;

    public static ServiceResult<T> Success(T value) => new(value, EmptyErrors);

    public static ServiceResult<T> Failure(IReadOnlyDictionary<string, string[]> errors) => new(null, errors);

    private static readonly IReadOnlyDictionary<string, string[]> EmptyErrors =
        new Dictionary<string, string[]>();
}
