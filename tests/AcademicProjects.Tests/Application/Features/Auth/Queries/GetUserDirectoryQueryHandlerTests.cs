using AcademicProjects.Application.Features.Auth.Queries;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Auth.Queries;

public class GetUserDirectoryQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsDirectoryFromUserManagementService()
    {
        var userManagementService = new FakeUserManagementService();
        var handler = new GetUserDirectoryQueryHandler(userManagementService);

        var result = await handler.Handle(new GetUserDirectoryQuery(), CancellationToken.None);

        Assert.Same(userManagementService.DirectoryResult, result);
    }
}
