using AcademicProjects.Application.Features.Auth.Queries;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Auth.Queries;

public class GetPendingUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPendingUsersFromUserManagementService()
    {
        var userManagementService = new FakeUserManagementService();
        var handler = new GetPendingUsersQueryHandler(userManagementService);

        var result = await handler.Handle(new GetPendingUsersQuery(), CancellationToken.None);

        Assert.Same(userManagementService.PendingUsersResult, result);
    }
}
