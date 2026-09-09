using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Tests.TestHelpers;

public sealed class TestCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; init; }

    public string? Email { get; init; } = "test@example.com";

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public bool IsAuthenticated { get; init; } = true;

    public static TestCurrentUserService AsAdministrator(Guid? userId = null) => new()
    {
        UserId = userId ?? Guid.NewGuid(),
        Roles = [UserRole.Administrator.ToString()]
    };

    public static TestCurrentUserService AsMentor(Guid? userId = null) => new()
    {
        UserId = userId ?? Guid.NewGuid(),
        Roles = [UserRole.Mentor.ToString()]
    };

    public static TestCurrentUserService AsStudent(Guid? userId = null) => new()
    {
        UserId = userId ?? Guid.NewGuid(),
        Roles = [UserRole.Student.ToString()]
    };
}
