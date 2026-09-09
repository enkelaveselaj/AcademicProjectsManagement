using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectMilestones.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class UpdateProjectMilestoneCommandHandlerTests
{
    [Fact]
    public async Task Handle_ProjectMentor_UpdatesAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var mentor = TestCurrentUserService.AsMentor();
        var mentorAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = mentor.UserId!.Value, Role = "Mentor" };
        var milestone = new ProjectMilestone { Title = "Old", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(mentorAssignment);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectMilestoneCommandHandler(context, mentor, new ProjectAccessService(context), new ProjectNotificationService(context));

        var result = await handler.Handle(
            new UpdateProjectMilestoneCommand(milestone.Id, " New Title ", project.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New Title", result!.Title);
    }

    [Fact]
    public async Task Handle_NonExistentMilestone_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdateProjectMilestoneCommandHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateProjectMilestoneCommand(Guid.NewGuid(), "Title", Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var mentor = TestCurrentUserService.AsMentor();
        var mentorAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = mentor.UserId!.Value, Role = "Mentor" };
        var milestone = new ProjectMilestone { Title = "Title", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(mentorAssignment);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectMilestoneCommandHandler(context, mentor, new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateProjectMilestoneCommand(milestone.Id, "Title", Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnrelatedMentor_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var milestone = new ProjectMilestone { Title = "Title", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectMilestoneCommandHandler(
            context,
            TestCurrentUserService.AsMentor(),
            new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new UpdateProjectMilestoneCommand(milestone.Id, "Title", project.Id),
                CancellationToken.None));
    }
}
