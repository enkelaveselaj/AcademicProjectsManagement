using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.Documents.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class GetDocumentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Administrator_ReturnsAllDocuments()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.AddRange(
            new Document { FileName = "a.pdf", FilePath = "/a.pdf", ProjectId = project.Id, Project = project },
            new Document { FileName = "b.pdf", FilePath = "/b.pdf", ProjectId = project.Id, Project = project });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetDocumentsQueryHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        var result = await handler.Handle(new GetDocumentsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_NoDocuments_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetDocumentsQueryHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        var result = await handler.Handle(new GetDocumentsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_NonMember_OnlySeesDocumentsFromAccessibleProjects()
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
        context.Documents.AddRange(
            new Document { FileName = "visible.pdf", FilePath = "/visible.pdf", ProjectId = memberProject.Id, Project = memberProject },
            new Document { FileName = "hidden.pdf", FilePath = "/hidden.pdf", ProjectId = otherProject.Id, Project = otherProject });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetDocumentsQueryHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(new GetDocumentsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("visible.pdf", result[0].FileName);
    }
}
