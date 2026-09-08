using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectAssignments.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectAssignments;

public class CreateProjectAssignmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProject_CreatesAssignmentAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateProjectAssignmentCommandHandler(context);
        var userId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateProjectAssignmentCommand(project.Id, userId, " Student "),
            CancellationToken.None);

        Assert.Equal(project.Id, result.ProjectId);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Student", result.Role);
        Assert.Single(context.ProjectAssignments);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateProjectAssignmentCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateProjectAssignmentCommand(Guid.NewGuid(), Guid.NewGuid(), "Student"),
                CancellationToken.None));
    }
}
