using AcademicProjects.Application.Features.Comments.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Comments;

public class GetCommentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllCommentsOrderedByCreatedAtDescending()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);

        var older = new Comment { Content = "Older", ProjectId = project.Id, Project = project, CreatedAt = DateTime.UtcNow.AddDays(-1) };
        var newer = new Comment { Content = "Newer", ProjectId = project.Id, Project = project, CreatedAt = DateTime.UtcNow };
        context.Comments.AddRange(older, newer);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetCommentsQueryHandler(context);

        var result = await handler.Handle(new GetCommentsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Newer", result[0].Content);
        Assert.Equal("Older", result[1].Content);
    }

    [Fact]
    public async Task Handle_NoComments_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetCommentsQueryHandler(context);

        var result = await handler.Handle(new GetCommentsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
