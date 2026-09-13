using AcademicProjects.Application.Authentication;
using AcademicProjects.Application.Features.Auth.Queries;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Auth.Queries;

public class GetUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsUsersFromUserManagementService()
    {
        var userManagementService = new FakeUserManagementService
        {
            UsersResult = [new UserSummary(Guid.NewGuid(), "First", "Last", "user@example.com", "Student")]
        };
        var handler = new GetUsersQueryHandler(userManagementService);

        var result = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

        Assert.Same(userManagementService.UsersResult, result);
    }
}
