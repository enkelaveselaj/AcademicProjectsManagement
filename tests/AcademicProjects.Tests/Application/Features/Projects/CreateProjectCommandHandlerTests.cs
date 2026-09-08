using AcademicProjects.Application.Features.Projects.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Projects;

public class CreateProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingCategory_CreatesProjectAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        context.Categories.Add(category);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateProjectCommandHandler(context);

        var result = await handler.Handle(
            new CreateProjectCommand(" Thesis ", " Description ", ProjectStatus.Draft, category.Id),
            CancellationToken.None);

        Assert.Equal("Thesis", result.Title);
        Assert.Equal("Description", result.Description);
        Assert.Equal(category.Id, result.CategoryId);
        Assert.Equal(category.Name, result.CategoryName);
        Assert.Single(context.Projects);
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ThrowsKeyNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateProjectCommandHandler(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(
                new CreateProjectCommand("Thesis", "Description", ProjectStatus.Draft, Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhitespaceDescription_StoresNull()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        context.Categories.Add(category);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateProjectCommandHandler(context);

        var result = await handler.Handle(
            new CreateProjectCommand("Thesis", "   ", ProjectStatus.Draft, category.Id),
            CancellationToken.None);

        Assert.Null(result.Description);
    }
}
