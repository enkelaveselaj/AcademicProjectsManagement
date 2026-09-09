using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.ProjectStatusHistories.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectStatusHistories;

public class GetProjectStatusHistoriesQueryHandlerTests
{
    [Fact]
    public async Task Handle_Administrator_ReturnsAllHistoriesOrderedByCreatedAtDescending()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Submitted, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);

        var older = new ProjectStatusHistory
        {
            ProjectId = project.Id,
            Project = project,
            PreviousStatus = ProjectStatus.Draft,
            NewStatus = ProjectStatus.Submitted,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var newer = new ProjectStatusHistory
        {
            ProjectId = project.Id,
            Project = project,
            PreviousStatus = ProjectStatus.Submitted,
            NewStatus = ProjectStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };
        context.ProjectStatusHistories.AddRange(older, newer);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectStatusHistoriesQueryHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectStatusHistoriesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(ProjectStatus.Approved, result[0].NewStatus);
        Assert.Equal(ProjectStatus.Submitted, result[1].NewStatus);
    }

    [Fact]
    public async Task Handle_NoHistories_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectStatusHistoriesQueryHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectStatusHistoriesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_NonMember_OnlySeesHistoryFromAccessibleProjects()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var memberProject = new Project { Title = "Member Project", Description = "Desc", Status = ProjectStatus.Submitted, CategoryId = category.Id, Category = category };
        var otherProject = new Project { Title = "Other Project", Description = "Desc", Status = ProjectStatus.Submitted, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var assignment = new ProjectAssignment { ProjectId = memberProject.Id, Project = memberProject, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.AddRange(memberProject, otherProject);
        context.ProjectAssignments.Add(assignment);
        context.ProjectStatusHistories.AddRange(
            new ProjectStatusHistory { ProjectId = memberProject.Id, Project = memberProject, PreviousStatus = ProjectStatus.Draft, NewStatus = ProjectStatus.Submitted },
            new ProjectStatusHistory { ProjectId = otherProject.Id, Project = otherProject, PreviousStatus = ProjectStatus.Draft, NewStatus = ProjectStatus.Submitted });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectStatusHistoriesQueryHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectStatusHistoriesQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(memberProject.Id, result[0].ProjectId);
    }
}
