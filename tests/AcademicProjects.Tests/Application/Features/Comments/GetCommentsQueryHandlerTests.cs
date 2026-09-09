using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.Comments.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Comments;

public class GetCommentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Administrator_ReturnsAllCommentsOrderedByCreatedAtDescending()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);

        var older = new Comment { Content = "Older", ProjectId = project.Id, Project = project, CreatedAt = DateTime.UtcNow.AddDays(-1) };
        var newer = new Comment { Content = "Newer", ProjectId = project.Id, Project = project, CreatedAt = DateTime.UtcNow };
        context.Comments.AddRange(older, newer);
        await context.SaveChangesAsync(CancellationToken.None);

        var admin = TestCurrentUserService.AsAdministrator();
        var handler = new GetCommentsQueryHandler(context, admin, new ProjectAccessService(context));

        var result = await handler.Handle(new GetCommentsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Newer", result[0].Content);
        Assert.Equal("Older", result[1].Content);
    }

    [Fact]
    public async Task Handle_NoComments_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetCommentsQueryHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        var result = await handler.Handle(new GetCommentsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_NonMember_OnlySeesCommentsFromAccessibleProjects()
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
        context.Comments.AddRange(
            new Comment { Content = "Visible", ProjectId = memberProject.Id, Project = memberProject },
            new Comment { Content = "Hidden", ProjectId = otherProject.Id, Project = otherProject });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetCommentsQueryHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(new GetCommentsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Visible", result[0].Content);
    }
}
