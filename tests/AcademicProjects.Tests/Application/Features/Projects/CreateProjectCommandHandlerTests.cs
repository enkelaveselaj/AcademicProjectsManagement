using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Projects.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Projects;

public class CreateProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_Mentor_CreatesProjectAndAutoAssignsCreatorAsMentor()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        context.Categories.Add(category);
        await context.SaveChangesAsync(CancellationToken.None);

        var mentor = TestCurrentUserService.AsMentor();
        var handler = new CreateProjectCommandHandler(context, mentor);

        var result = await handler.Handle(
            new CreateProjectCommand(" Thesis ", " Description ", ProjectStatus.Draft, category.Id),
            CancellationToken.None);

        Assert.Equal("Thesis", result.Title);
        Assert.Equal("Description", result.Description);
        Assert.Equal(category.Id, result.CategoryId);
        Assert.Equal(category.Name, result.CategoryName);
        Assert.Equal(mentor.UserId, result.CreatedById);
        Assert.Single(context.Projects);

        var assignment = Assert.Single(context.ProjectAssignments);
        Assert.Equal(mentor.UserId, assignment.UserId);
        Assert.Equal("Mentor", assignment.Role);
    }

    [Fact]
    public async Task Handle_Student_CreatesProjectAndAutoAssignsCreatorAsStudent()
    {
        // Students can start and run their own projects, solo or as a team.
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        context.Categories.Add(category);
        await context.SaveChangesAsync(CancellationToken.None);

        var student = TestCurrentUserService.AsStudent();
        var handler = new CreateProjectCommandHandler(context, student);

        var result = await handler.Handle(
            new CreateProjectCommand("Thesis", "Description", ProjectStatus.Draft, category.Id),
            CancellationToken.None);

        Assert.Equal(student.UserId, result.CreatedById);

        var assignment = Assert.Single(context.ProjectAssignments);
        Assert.Equal(student.UserId, assignment.UserId);
        Assert.Equal("Student", assignment.Role);
    }

    [Fact]
    public async Task Handle_Administrator_CreatesProjectWithoutAutoAssignment()
    {
        // Administrators manage the platform - they aren't project participants.
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        context.Categories.Add(category);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateProjectCommandHandler(context, TestCurrentUserService.AsAdministrator());

        await handler.Handle(
            new CreateProjectCommand("Thesis", "Description", ProjectStatus.Draft, category.Id),
            CancellationToken.None);

        Assert.Empty(context.ProjectAssignments);
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateProjectCommandHandler(context, TestCurrentUserService.AsAdministrator());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateProjectCommand("Thesis", "Description", ProjectStatus.Draft, Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhitespaceDescription_StoresNull()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        context.Categories.Add(category);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateProjectCommandHandler(context, TestCurrentUserService.AsAdministrator());

        var result = await handler.Handle(
            new CreateProjectCommand("Thesis", "   ", ProjectStatus.Draft, category.Id),
            CancellationToken.None);

        Assert.Null(result.Description);
    }
}
