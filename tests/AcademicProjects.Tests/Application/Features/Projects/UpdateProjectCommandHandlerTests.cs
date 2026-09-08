using AcademicProjects.Application.Features.Projects.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Projects;

public class UpdateProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProject_UpdatesAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Old", Description = "Old", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectCommandHandler(context);

        var result = await handler.Handle(
            new UpdateProjectCommand(project.Id, " New Title ", " New Description ", ProjectStatus.Draft, category.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New Title", result!.Title);
        Assert.Equal("New Description", result.Description);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ReturnsNull()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdateProjectCommandHandler(context);

        var result = await handler.Handle(
            new UpdateProjectCommand(Guid.NewGuid(), "Title", "Description", ProjectStatus.Draft, Guid.NewGuid()),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ThrowsKeyNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectCommandHandler(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(
                new UpdateProjectCommand(project.Id, "Title", "Description", ProjectStatus.Draft, Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_StatusChanged_RecordsProjectStatusHistory()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectCommandHandler(context);

        await handler.Handle(
            new UpdateProjectCommand(project.Id, "Title", "Description", ProjectStatus.Submitted, category.Id, " Ready for review "),
            CancellationToken.None);

        var history = Assert.Single(context.ProjectStatusHistories);
        Assert.Equal(project.Id, history.ProjectId);
        Assert.Equal(ProjectStatus.Draft, history.PreviousStatus);
        Assert.Equal(ProjectStatus.Submitted, history.NewStatus);
        Assert.Equal("Ready for review", history.Comment);
    }

    [Fact]
    public async Task Handle_StatusUnchanged_DoesNotRecordProjectStatusHistory()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectCommandHandler(context);

        await handler.Handle(
            new UpdateProjectCommand(project.Id, "New Title", "Description", ProjectStatus.Draft, category.Id),
            CancellationToken.None);

        Assert.Empty(context.ProjectStatusHistories);
    }
}
