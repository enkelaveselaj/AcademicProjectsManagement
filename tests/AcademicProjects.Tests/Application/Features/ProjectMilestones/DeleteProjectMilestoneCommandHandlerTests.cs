using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectMilestones.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class DeleteProjectMilestoneCommandHandlerTests
{
    [Fact]
    public async Task Handle_ProjectMentor_RemovesIt()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var mentor = TestCurrentUserService.AsMentor();
        var mentorAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = mentor.UserId!.Value, Role = "Mentor" };
        var milestone = new ProjectMilestone { Title = "Milestone", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(mentorAssignment);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteProjectMilestoneCommandHandler(context, mentor, new ProjectAccessService(context));

        await handler.Handle(
            new DeleteProjectMilestoneCommand(milestone.Id),
            CancellationToken.None);

        Assert.Empty(context.ProjectMilestones);
    }

    [Fact]
    public async Task Handle_NonExistentMilestone_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteProjectMilestoneCommandHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new DeleteProjectMilestoneCommand(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnrelatedMentor_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var milestone = new ProjectMilestone { Title = "Milestone", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteProjectMilestoneCommandHandler(
            context,
            TestCurrentUserService.AsMentor(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new DeleteProjectMilestoneCommand(milestone.Id),
                CancellationToken.None));
    }
}
