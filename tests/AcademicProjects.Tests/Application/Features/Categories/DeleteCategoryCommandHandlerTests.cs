using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Categories.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Categories;

public class DeleteCategoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingCategory_RemovesIt()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        context.Categories.Add(category);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteCategoryCommandHandler(context);

        await handler.Handle(
            new DeleteCategoryCommand(category.Id),
            CancellationToken.None);

        Assert.Empty(context.Categories);
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteCategoryCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new DeleteCategoryCommand(Guid.NewGuid()),
                CancellationToken.None));
    }
}
