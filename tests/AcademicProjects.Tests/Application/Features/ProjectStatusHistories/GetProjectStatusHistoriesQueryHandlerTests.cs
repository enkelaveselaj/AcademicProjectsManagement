using AcademicProjects.Application.Features.ProjectStatusHistories.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectStatusHistories;

public class GetProjectStatusHistoriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllHistoriesOrderedByCreatedAtDescending()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Submitted, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);

        var older = new ProjectStatusHistory
        {
            ProjectId = project.Id,
            Project = project,
            PreviousStatus = ProjectStatus.Draft,
            NewStatus = ProjectStatus.Submitted,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var newer = new ProjectStatusHistory
        {
            ProjectId = project.Id,
            Project = project,
            PreviousStatus = ProjectStatus.Submitted,
            NewStatus = ProjectStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };
        context.ProjectStatusHistories.AddRange(older, newer);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectStatusHistoriesQueryHandler(context);

        var result = await handler.Handle(new GetProjectStatusHistoriesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(ProjectStatus.Approved, result[0].NewStatus);
        Assert.Equal(ProjectStatus.Submitted, result[1].NewStatus);
    }

    [Fact]
    public async Task Handle_NoHistories_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectStatusHistoriesQueryHandler(context);

        var result = await handler.Handle(new GetProjectStatusHistoriesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
