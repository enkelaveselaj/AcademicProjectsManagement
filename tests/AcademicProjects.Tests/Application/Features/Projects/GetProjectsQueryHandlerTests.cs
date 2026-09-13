using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.Projects.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Projects;

public class GetProjectsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Administrator_ReturnsAllProjectsOrderedByTitle()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        context.Categories.Add(category);
        context.Projects.AddRange(
            new Project { Title = "Zebra Study", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category },
            new Project { Title = "Ant Study", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectsQueryHandler(context, TestCurrentUserService.AsAdministrator(), new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Ant Study", result[0].Title);
        Assert.Equal("Zebra Study", result[1].Title);
    }

    [Fact]
    public async Task Handle_NoProjects_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectsQueryHandler(context, TestCurrentUserService.AsAdministrator(), new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_NonMember_OnlySeesAccessibleProjects()
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
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectsQueryHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Member Project", result[0].Title);
    }
}
