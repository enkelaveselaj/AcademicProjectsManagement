using AcademicProjects.Application.Authentication;

namespace AcademicProjects.Tests.Application;

public class ServiceResultTests
{
    [Fact]
    public void Success_HasValueAndNoErrors()
    {
        var user = new RegisteredUser(Guid.NewGuid(), "student@example.com", "Student");

        var result = ServiceResult<RegisteredUser>.Success(user);

        Assert.True(result.Succeeded);
        Assert.Equal(user, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_HasErrorsAndNoValue()
    {
        var result = ServiceResult<AccessToken>.Failure(new Dictionary<string, string[]>
        {
            ["credentials"] = ["Invalid email or password."]
        });

        Assert.False(result.Succeeded);
        Assert.Null(result.Value);
        Assert.Contains("credentials", result.Errors.Keys);
    }
}
