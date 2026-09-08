using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Categories.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Categories;

public class UpdateCategoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingCategory_UpdatesAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Old Name", Description = "Old" };
        context.Categories.Add(category);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateCategoryCommandHandler(context);

        var result = await handler.Handle(
            new UpdateCategoryCommand(category.Id, " New Name ", " New Description "),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New Name", result!.Name);
        Assert.Equal("New Description", result.Description);
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdateCategoryCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateCategoryCommand(Guid.NewGuid(), "Name", "Description"),
                CancellationToken.None));
    }
}
