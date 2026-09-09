using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.ProjectAssignments.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectAssignments;

public class GetProjectAssignmentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Administrator_ReturnsAllAssignments()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.AddRange(
            new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = Guid.NewGuid(), Role = "Student" },
            new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = Guid.NewGuid(), Role = "Mentor" });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectAssignmentsQueryHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectAssignmentsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_NoAssignments_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectAssignmentsQueryHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectAssignmentsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ProjectMember_OnlySeesAssignmentsFromAccessibleProjects()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var memberProject = new Project { Title = "Member Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var otherProject = new Project { Title = "Other Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var ownAssignment = new ProjectAssignment { ProjectId = memberProject.Id, Project = memberProject, UserId = student.UserId!.Value, Role = "Student" };
        var otherAssignment = new ProjectAssignment { ProjectId = otherProject.Id, Project = otherProject, UserId = Guid.NewGuid(), Role = "Student" };
        context.Categories.Add(category);
        context.Projects.AddRange(memberProject, otherProject);
        context.ProjectAssignments.AddRange(ownAssignment, otherAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectAssignmentsQueryHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectAssignmentsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(memberProject.Id, result[0].ProjectId);
    }
}
