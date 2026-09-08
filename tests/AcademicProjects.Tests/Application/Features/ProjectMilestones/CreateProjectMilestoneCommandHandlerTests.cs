using AcademicProjects.Application.Features.ProjectMilestones.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class CreateProjectMilestoneCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProject_CreatesMilestoneAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateProjectMilestoneCommandHandler(context);

        var result = await handler.Handle(
            new CreateProjectMilestoneCommand(" Literature Review ", project.Id),
            CancellationToken.None);

        Assert.Equal("Literature Review", result.Title);
        Assert.Equal(project.Id, result.ProjectId);
        Assert.Single(context.ProjectMilestones);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsKeyNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateProjectMilestoneCommandHandler(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(
                new CreateProjectMilestoneCommand("Milestone", Guid.NewGuid()),
                CancellationToken.None));
    }
}
