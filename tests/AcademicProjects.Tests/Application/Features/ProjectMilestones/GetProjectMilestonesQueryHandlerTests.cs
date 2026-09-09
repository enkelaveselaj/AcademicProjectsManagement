using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.ProjectMilestones.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class GetProjectMilestonesQueryHandlerTests
{
    [Fact]
    public async Task Handle_Administrator_ReturnsAllMilestonesOrderedByCreatedAtDescending()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);

        var older = new ProjectMilestone { Title = "Older", ProjectId = project.Id, Project = project, CreatedAt = DateTime.UtcNow.AddDays(-1) };
        var newer = new ProjectMilestone { Title = "Newer", ProjectId = project.Id, Project = project, CreatedAt = DateTime.UtcNow };
        context.ProjectMilestones.AddRange(older, newer);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectMilestonesQueryHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectMilestonesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Newer", result[0].Title);
        Assert.Equal("Older", result[1].Title);
    }

    [Fact]
    public async Task Handle_NoMilestones_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectMilestonesQueryHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectMilestonesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_NonMember_OnlySeesMilestonesFromAccessibleProjects()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var memberProject = new Project { Title = "Member Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var otherProject = new Project { Title = "Other Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var assignment = new ProjectAssignment { ProjectId = memberProject.Id, Project = memberProject, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.AddRange(memberProject, otherProject);
        context.ProjectAssignments.Add(assignment);
        context.ProjectMilestones.AddRange(
            new ProjectMilestone { Title = "Visible", ProjectId = memberProject.Id, Project = memberProject },
            new ProjectMilestone { Title = "Hidden", ProjectId = otherProject.Id, Project = otherProject });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectMilestonesQueryHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectMilestonesQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Visible", result[0].Title);
    }
}
