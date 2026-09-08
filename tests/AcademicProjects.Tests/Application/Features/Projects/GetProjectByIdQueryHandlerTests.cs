using AcademicProjects.Application.Features.Projects.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Projects;

public class GetProjectByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProject_ReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetProjectByIdQuery(project.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(project.Id, result!.Id);
        Assert.Equal(category.Name, result.CategoryName);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ReturnsNull()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetProjectByIdQuery(Guid.NewGuid()),
            CancellationToken.None);

        Assert.Null(result);
    }
}
