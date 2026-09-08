using AcademicProjects.Application.Features.ProjectMilestones.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class GetProjectMilestonesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllMilestonesOrderedByCreatedAtDescending()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);

        var older = new ProjectMilestone { Title = "Older", ProjectId = project.Id, Project = project, CreatedAt = DateTime.UtcNow.AddDays(-1) };
        var newer = new ProjectMilestone { Title = "Newer", ProjectId = project.Id, Project = project, CreatedAt = DateTime.UtcNow };
        context.ProjectMilestones.AddRange(older, newer);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectMilestonesQueryHandler(context);

        var result = await handler.Handle(new GetProjectMilestonesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Newer", result[0].Title);
        Assert.Equal("Older", result[1].Title);
    }

    [Fact]
    public async Task Handle_NoMilestones_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectMilestonesQueryHandler(context);

        var result = await handler.Handle(new GetProjectMilestonesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
