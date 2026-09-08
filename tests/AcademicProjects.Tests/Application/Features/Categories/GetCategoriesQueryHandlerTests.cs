using AcademicProjects.Application.Features.Categories.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Categories;

public class GetCategoriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllCategoriesOrderedByName()
    {
        using var context = TestDbContextFactory.Create();
        context.Categories.AddRange(
            new Category { Name = "Zoology" },
            new Category { Name = "Algorithms" });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetCategoriesQueryHandler(context);

        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Algorithms", result[0].Name);
        Assert.Equal("Zoology", result[1].Name);
    }

    [Fact]
    public async Task Handle_NoCategories_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetCategoriesQueryHandler(context);

        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
