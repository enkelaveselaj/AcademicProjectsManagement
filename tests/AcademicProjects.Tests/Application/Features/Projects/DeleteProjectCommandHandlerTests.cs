using AcademicProjects.Application.Features.Projects.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Projects;

public class DeleteProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProject_RemovesItAndReturnsTrue()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteProjectCommandHandler(context);

        var result = await handler.Handle(
            new DeleteProjectCommand(project.Id),
            CancellationToken.None);

        Assert.True(result);
        Assert.Empty(context.Projects);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ReturnsFalse()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteProjectCommandHandler(context);

        var result = await handler.Handle(
            new DeleteProjectCommand(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result);
    }
}
