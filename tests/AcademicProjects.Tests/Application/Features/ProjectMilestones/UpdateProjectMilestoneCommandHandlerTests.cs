using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectMilestones.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class UpdateProjectMilestoneCommandHandlerTests
{
    private static readonly DateTime FutureDueDate = DateTime.UtcNow.AddDays(30);

    [Fact]
    public async Task Handle_ProjectMentor_UpdatesAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var mentor = TestCurrentUserService.AsMentor();
        var mentorAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = mentor.UserId!.Value, Role = "Mentor" };
        var milestone = new ProjectMilestone { Title = "Old", DueDate = FutureDueDate, ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(mentorAssignment);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectMilestoneCommandHandler(context, mentor, new ProjectNotificationService(context));

        var result = await handler.Handle(
            new UpdateProjectMilestoneCommand(milestone.Id, " New Title ", "New description", FutureDueDate, MilestoneStatus.InProgress, project.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New Title", result!.Title);
        Assert.Equal("New description", result.Description);
        Assert.Equal(MilestoneStatus.InProgress, result.Status);
    }

    [Fact]
    public async Task Handle_NonExistentMilestone_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdateProjectMilestoneCommandHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateProjectMilestoneCommand(Guid.NewGuid(), "Title", null, FutureDueDate, MilestoneStatus.Pending, Guid.NewGuid()),
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
        var milestone = new ProjectMilestone { Title = "Title", DueDate = FutureDueDate, ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(mentorAssignment);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectMilestoneCommandHandler(context, mentor, new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateProjectMilestoneCommand(milestone.Id, "Title", null, FutureDueDate, MilestoneStatus.Pending, Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ProjectIdDoesNotMatchMilestonesProject_ThrowsNotFoundException()
    {
        // A milestone can't be "reparented" to a different project by sending a mismatched
        // ProjectId - that's treated the same as the milestone not existing at all.
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var otherProject = new Project { Title = "Other", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var mentor = TestCurrentUserService.AsMentor();
        var mentorAssignment = new ProjectAssignment { ProjectId = otherProject.Id, Project = otherProject, UserId = mentor.UserId!.Value, Role = "Mentor" };
        var milestone = new ProjectMilestone { Title = "Title", DueDate = FutureDueDate, ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.AddRange(project, otherProject);
        context.ProjectAssignments.Add(mentorAssignment);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectMilestoneCommandHandler(context, mentor, new ProjectNotificationService(context));

        // The caller is the mentor of otherProject, and tries to update a milestone that
        // actually belongs to project by claiming it belongs to otherProject instead.
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateProjectMilestoneCommand(milestone.Id, "Title", null, FutureDueDate, MilestoneStatus.Pending, otherProject.Id),
                CancellationToken.None));

        Assert.Equal(project.Id, milestone.ProjectId);
    }

    [Fact]
    public async Task Handle_TransitionToCompleted_SetsCompletedAt()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var mentor = TestCurrentUserService.AsMentor();
        var mentorAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = mentor.UserId!.Value, Role = "Mentor" };
        var milestone = new ProjectMilestone { Title = "Title", DueDate = FutureDueDate, Status = MilestoneStatus.InProgress, ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(mentorAssignment);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectMilestoneCommandHandler(context, mentor, new ProjectNotificationService(context));

        var result = await handler.Handle(
            new UpdateProjectMilestoneCommand(milestone.Id, "Title", null, FutureDueDate, MilestoneStatus.Completed, project.Id),
            CancellationToken.None);

        Assert.Equal(MilestoneStatus.Completed, result.Status);
        Assert.NotNull(result.CompletedAt);
    }

    [Fact]
    public async Task Handle_TransitionAwayFromCompleted_ClearsCompletedAt()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var mentor = TestCurrentUserService.AsMentor();
        var mentorAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = mentor.UserId!.Value, Role = "Mentor" };
        var milestone = new ProjectMilestone
        {
            Title = "Title",
            DueDate = FutureDueDate,
            Status = MilestoneStatus.Completed,
            CompletedAt = DateTime.UtcNow.AddDays(-1),
            ProjectId = project.Id,
            Project = project
        };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(mentorAssignment);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateProjectMilestoneCommandHandler(context, mentor, new ProjectNotificationService(context));

        var result = await handler.Handle(
            new UpdateProjectMilestoneCommand(milestone.Id, "Title", null, FutureDueDate, MilestoneStatus.InProgress, project.Id),
            CancellationToken.None);

        Assert.Equal(MilestoneStatus.InProgress, result.Status);
        Assert.Null(result.CompletedAt);
    }
}
