using AcademicProjects.Application.Features.ProjectMilestones.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class GetProjectMilestoneByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ExistingMilestone_ReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var milestone = new ProjectMilestone { Title = "Milestone", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectMilestoneByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetProjectMilestoneByIdQuery(milestone.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(milestone.Id, result!.Id);
    }

    [Fact]
    public async Task Handle_NonExistentMilestone_ReturnsNull()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectMilestoneByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetProjectMilestoneByIdQuery(Guid.NewGuid()),
            CancellationToken.None);

        Assert.Null(result);
    }
}
