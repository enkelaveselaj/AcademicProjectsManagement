using AcademicProjects.Application.Features.Categories.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Categories;

public class GetCategoryByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ExistingCategory_ReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category", Description = "Desc" };
        context.Categories.Add(category);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetCategoryByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetCategoryByIdQuery(category.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(category.Id, result!.Id);
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ReturnsNull()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetCategoryByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetCategoryByIdQuery(Guid.NewGuid()),
            CancellationToken.None);

        Assert.Null(result);
    }
}
