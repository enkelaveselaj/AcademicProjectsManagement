using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.Projects.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Projects;

public class UpdateProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_ProjectMentor_UpdatesAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Old", Description = "Old", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var mentor = TestCurrentUserService.AsMentor();
        var mentorAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = mentor.UserId!.Value, Role = "Mentor" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(mentorAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectCommandHandler(context, mentor, new ProjectAccessService(context), new ProjectNotificationService(context));

        var result = await handler.Handle(
            new UpdateProjectCommand(project.Id, " New Title ", " New Description ", ProjectStatus.Draft, category.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New Title", result!.Title);
        Assert.Equal("New Description", result.Description);
    }

    [Fact]
    public async Task Handle_ProjectMemberStudent_UpdatesAndReturnsDto()
    {
        // A student running their own project needs to be able to edit it, not just the mentor.
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Old", Description = "Old", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var studentAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(studentAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectCommandHandler(context, student, new ProjectAccessService(context), new ProjectNotificationService(context));

        var result = await handler.Handle(
            new UpdateProjectCommand(project.Id, "New Title", "New Description", ProjectStatus.Draft, category.Id),
            CancellationToken.None);

        Assert.Equal("New Title", result.Title);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdateProjectCommandHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateProjectCommand(Guid.NewGuid(), "Title", "Description", ProjectStatus.Draft, Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var admin = TestCurrentUserService.AsAdministrator();
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectCommandHandler(context, admin, new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateProjectCommand(project.Id, "Title", "Description", ProjectStatus.Draft, Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_StatusChanged_RecordsProjectStatusHistory()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var admin = TestCurrentUserService.AsAdministrator();
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectCommandHandler(context, admin, new ProjectAccessService(context), new ProjectNotificationService(context));

        await handler.Handle(
            new UpdateProjectCommand(project.Id, "Title", "Description", ProjectStatus.Submitted, category.Id, " Ready for review "),
            CancellationToken.None);

        var history = Assert.Single(context.ProjectStatusHistories);
        Assert.Equal(project.Id, history.ProjectId);
        Assert.Equal(ProjectStatus.Draft, history.PreviousStatus);
        Assert.Equal(ProjectStatus.Submitted, history.NewStatus);
        Assert.Equal("Ready for review", history.Comment);
    }

    [Fact]
    public async Task Handle_StatusChanged_NotifiesOtherMembersWithOldAndNewStatus()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var mentor = TestCurrentUserService.AsMentor();
        var student = TestCurrentUserService.AsStudent();
        var mentorAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = mentor.UserId!.Value, Role = "Mentor" };
        var studentAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.AddRange(mentorAssignment, studentAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectCommandHandler(context, mentor, new ProjectAccessService(context), new ProjectNotificationService(context));

        await handler.Handle(
            new UpdateProjectCommand(project.Id, "Title", "Description", ProjectStatus.Submitted, category.Id),
            CancellationToken.None);

        var notification = Assert.Single(context.Notifications);
        Assert.Equal(student.UserId, notification.UserId);
        Assert.Contains("Draft", notification.Message);
        Assert.Contains("Submitted", notification.Message);
    }

    [Fact]
    public async Task Handle_StatusUnchanged_DoesNotRecordProjectStatusHistory()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var admin = TestCurrentUserService.AsAdministrator();
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectCommandHandler(context, admin, new ProjectAccessService(context), new ProjectNotificationService(context));

        await handler.Handle(
            new UpdateProjectCommand(project.Id, "New Title", "Description", ProjectStatus.Draft, category.Id),
            CancellationToken.None);

        Assert.Empty(context.ProjectStatusHistories);
    }

    [Fact]
    public async Task Handle_UnrelatedMentor_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectCommandHandler(
            context,
            TestCurrentUserService.AsMentor(),
            new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new UpdateProjectCommand(project.Id, "Title", "Description", ProjectStatus.Draft, category.Id),
                CancellationToken.None));
    }
}
