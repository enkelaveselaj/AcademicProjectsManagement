using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectMilestones.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class CreateProjectMilestoneCommandHandlerTests
{
    private static readonly DateTime FutureDueDate = DateTime.UtcNow.AddDays(30);

    [Fact]
    public async Task Handle_ProjectMentor_CreatesMilestoneAndReturnsDto()
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

        var handler = new CreateProjectMilestoneCommandHandler(context, mentor, new ProjectAccessService(context), new ProjectNotificationService(context));

        var result = await handler.Handle(
            new CreateProjectMilestoneCommand(" Literature Review ", "Review key papers", FutureDueDate, project.Id),
            CancellationToken.None);

        Assert.Equal("Literature Review", result.Title);
        Assert.Equal("Review key papers", result.Description);
        Assert.Equal(MilestoneStatus.Pending, result.Status);
        Assert.Null(result.CompletedAt);
        Assert.Equal(project.Id, result.ProjectId);
        Assert.Single(context.ProjectMilestones);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateProjectMilestoneCommandHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateProjectMilestoneCommand("Milestone", null, FutureDueDate, Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ProjectMemberStudent_CreatesMilestoneAndReturnsDto()
    {
        // Student-run projects need to be able to set their own milestones too.
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateProjectMilestoneCommandHandler(context, student, new ProjectAccessService(context), new ProjectNotificationService(context));

        var result = await handler.Handle(
            new CreateProjectMilestoneCommand("Milestone", null, FutureDueDate, project.Id),
            CancellationToken.None);

        Assert.Equal("Milestone", result.Title);
        Assert.Single(context.ProjectMilestones);
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

        var handler = new CreateProjectMilestoneCommandHandler(
            context,
            TestCurrentUserService.AsStudent(),
            new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new CreateProjectMilestoneCommand("Milestone", null, FutureDueDate, project.Id),
                CancellationToken.None));
    }
}
