using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectAssignments.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectAssignments;

public class CreateProjectAssignmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ProjectMentor_CreatesAssignmentAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var mentor = TestCurrentUserService.AsMentor();
        var mentorAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = mentor.UserId!.Value, Role = "Mentor" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(mentorAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateProjectAssignmentCommandHandler(context, mentor, new ProjectAccessService(context));
        var userId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateProjectAssignmentCommand(project.Id, userId, " Student "),
            CancellationToken.None);

        Assert.Equal(project.Id, result.ProjectId);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Student", result.Role);
        Assert.Equal(2, context.ProjectAssignments.Count());
    }

    [Fact]
    public async Task Handle_ProjectMemberStudent_AssignsMentorAndReturnsDto()
    {
        // Students who create their own project need to be able to bring a mentor onto it.
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var studentAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(studentAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateProjectAssignmentCommandHandler(context, student, new ProjectAccessService(context));
        var mentorUserId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateProjectAssignmentCommand(project.Id, mentorUserId, " Mentor "),
            CancellationToken.None);

        Assert.Equal(mentorUserId, result.UserId);
        Assert.Equal("Mentor", result.Role);
        Assert.Equal(2, context.ProjectAssignments.Count());
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateProjectAssignmentCommandHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateProjectAssignmentCommand(Guid.NewGuid(), Guid.NewGuid(), "Student"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnrelatedStudent_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateProjectAssignmentCommandHandler(
            context,
            TestCurrentUserService.AsStudent(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new CreateProjectAssignmentCommand(project.Id, Guid.NewGuid(), "Student"),
                CancellationToken.None));
    }
}
