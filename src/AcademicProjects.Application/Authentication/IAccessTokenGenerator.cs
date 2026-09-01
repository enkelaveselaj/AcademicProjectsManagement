namespace AcademicProjects.Application.Authentication;

public interface IAccessTokenGenerator
{
    AccessToken Generate(
        Guid userId,
        string email,
        string firstName,
        string lastName,
        IEnumerable<string> roles);
}
