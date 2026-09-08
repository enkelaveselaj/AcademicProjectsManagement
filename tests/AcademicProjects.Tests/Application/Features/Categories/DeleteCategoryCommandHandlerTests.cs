using AcademicProjects.Application.Features.Categories.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Categories;

public class DeleteCategoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingCategory_RemovesItAndReturnsTrue()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        context.Categories.Add(category);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteCategoryCommandHandler(context);

        var result = await handler.Handle(
            new DeleteCategoryCommand(category.Id),
            CancellationToken.None);

        Assert.True(result);
        Assert.Empty(context.Categories);
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ReturnsFalse()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteCategoryCommandHandler(context);

        var result = await handler.Handle(
            new DeleteCategoryCommand(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result);
    }
}
