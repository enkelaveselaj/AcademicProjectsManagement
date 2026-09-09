using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Categories.Commands;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Categories;

public class CreateCategoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesCategoryAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateCategoryCommandHandler(context, TestCurrentUserService.AsAdministrator());

        var result = await handler.Handle(
            new CreateCategoryCommand(" Machine Learning ", " AI related projects "),
            CancellationToken.None);

        Assert.Equal("Machine Learning", result.Name);
        Assert.Equal("AI related projects", result.Description);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Single(context.Categories);
    }

    [Fact]
    public async Task Handle_WhitespaceDescription_StoresNull()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateCategoryCommandHandler(context, TestCurrentUserService.AsAdministrator());

        var result = await handler.Handle(
            new CreateCategoryCommand("Web Development", "   "),
            CancellationToken.None);

        Assert.Null(result.Description);
    }

    [Fact]
    public async Task Handle_NonAdministrator_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateCategoryCommandHandler(context, TestCurrentUserService.AsMentor());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new CreateCategoryCommand("Category", null),
                CancellationToken.None));
    }
}
