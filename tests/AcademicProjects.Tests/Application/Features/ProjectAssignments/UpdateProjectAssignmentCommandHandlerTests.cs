using AcademicProjects.Application.Features.ProjectAssignments.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectAssignments;

public class UpdateProjectAssignmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingAssignment_UpdatesAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = Guid.NewGuid(), Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectAssignmentCommandHandler(context);
        var newUserId = Guid.NewGuid();

        var result = await handler.Handle(
            new UpdateProjectAssignmentCommand(assignment.Id, project.Id, newUserId, " Mentor "),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(newUserId, result!.UserId);
        Assert.Equal("Mentor", result.Role);
    }

    [Fact]
    public async Task Handle_NonExistentAssignment_ReturnsNull()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdateProjectAssignmentCommandHandler(context);

        var result = await handler.Handle(
            new UpdateProjectAssignmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Student"),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsKeyNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = Guid.NewGuid(), Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectAssignmentCommandHandler(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(
                new UpdateProjectAssignmentCommand(assignment.Id, Guid.NewGuid(), Guid.NewGuid(), "Student"),
                CancellationToken.None));
    }
}
