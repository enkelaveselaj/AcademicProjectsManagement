using AcademicProjects.Application.Features.ProjectAssignments.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectAssignments;

public class DeleteProjectAssignmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingAssignment_RemovesItAndReturnsTrue()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = Guid.NewGuid(), Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteProjectAssignmentCommandHandler(context);

        var result = await handler.Handle(
            new DeleteProjectAssignmentCommand(assignment.Id),
            CancellationToken.None);

        Assert.True(result);
        Assert.Empty(context.ProjectAssignments);
    }

    [Fact]
    public async Task Handle_NonExistentAssignment_ReturnsFalse()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteProjectAssignmentCommandHandler(context);

        var result = await handler.Handle(
            new DeleteProjectAssignmentCommand(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result);
    }
}
