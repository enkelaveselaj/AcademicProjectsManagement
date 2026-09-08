using AcademicProjects.Application.Features.ProjectMilestones.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class UpdateProjectMilestoneCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingMilestone_UpdatesAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var milestone = new ProjectMilestone { Title = "Old", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectMilestoneCommandHandler(context);

        var result = await handler.Handle(
            new UpdateProjectMilestoneCommand(milestone.Id, " New Title ", project.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New Title", result!.Title);
    }

    [Fact]
    public async Task Handle_NonExistentMilestone_ReturnsNull()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdateProjectMilestoneCommandHandler(context);

        var result = await handler.Handle(
            new UpdateProjectMilestoneCommand(Guid.NewGuid(), "Title", Guid.NewGuid()),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsKeyNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var milestone = new ProjectMilestone { Title = "Title", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectMilestoneCommandHandler(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(
                new UpdateProjectMilestoneCommand(milestone.Id, "Title", Guid.NewGuid()),
                CancellationToken.None));
    }
}
