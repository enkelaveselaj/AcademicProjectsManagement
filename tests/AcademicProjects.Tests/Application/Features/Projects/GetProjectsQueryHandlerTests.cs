using AcademicProjects.Application.Features.Projects.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Projects;

public class GetProjectsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllProjectsOrderedByTitle()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        context.Categories.Add(category);
        context.Projects.AddRange(
            new Project { Title = "Zebra Study", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category },
            new Project { Title = "Ant Study", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectsQueryHandler(context);

        var result = await handler.Handle(new GetProjectsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Ant Study", result[0].Title);
        Assert.Equal("Zebra Study", result[1].Title);
    }

    [Fact]
    public async Task Handle_NoProjects_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectsQueryHandler(context);

        var result = await handler.Handle(new GetProjectsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
