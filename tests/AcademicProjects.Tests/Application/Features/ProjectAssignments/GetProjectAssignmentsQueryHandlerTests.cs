using AcademicProjects.Application.Features.ProjectAssignments.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectAssignments;

public class GetProjectAssignmentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllAssignments()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.AddRange(
            new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = Guid.NewGuid(), Role = "Student" },
            new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = Guid.NewGuid(), Role = "Mentor" });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectAssignmentsQueryHandler(context);

        var result = await handler.Handle(new GetProjectAssignmentsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_NoAssignments_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectAssignmentsQueryHandler(context);

        var result = await handler.Handle(new GetProjectAssignmentsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
