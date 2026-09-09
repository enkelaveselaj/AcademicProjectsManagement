using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Comments.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Comments;

public class CreateCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ProjectMember_CreatesCommentAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateCommentCommandHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(
            new CreateCommentCommand(" Looks good ", project.Id),
            CancellationToken.None);

        Assert.Equal("Looks good", result.Content);
        Assert.Equal(student.UserId, result.AuthorId);
        Assert.Equal(project.Id, result.ProjectId);
        Assert.Single(context.Comments);
    }

    [Fact]
    public async Task Handle_ProjectMentor_CreatesCommentAndReturnsDto()
    {
        // Leaving feedback on a project's work is the mentor's core job.
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var mentor = TestCurrentUserService.AsMentor();
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = mentor.UserId!.Value, Role = "Mentor" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateCommentCommandHandler(context, mentor, new ProjectAccessService(context));

        var result = await handler.Handle(
            new CreateCommentCommand("Good progress, keep it up.", project.Id),
            CancellationToken.None);

        Assert.Equal(mentor.UserId, result.AuthorId);
        Assert.Single(context.Comments);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateCommentCommandHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateCommentCommand("Content", Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateCommentCommandHandler(
            context,
            TestCurrentUserService.AsStudent(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new CreateCommentCommand("Content", project.Id),
                CancellationToken.None));
    }
}
