using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Documents.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class GetDocumentByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ProjectMember_ReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var document = new Document { FileName = "file.pdf", StoredFileName = "stored-file.pdf", ContentType = "application/pdf", FileSizeBytes = 10, ProjectId = project.Id, Project = project };
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetDocumentByIdQueryHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(
            new GetDocumentByIdQuery(document.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(document.Id, result!.Id);
    }

    [Fact]
    public async Task Handle_NonExistentDocument_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetDocumentByIdQueryHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new GetDocumentByIdQuery(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var document = new Document { FileName = "file.pdf", StoredFileName = "stored-file.pdf", ContentType = "application/pdf", FileSizeBytes = 10, ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetDocumentByIdQueryHandler(
            context,
            TestCurrentUserService.AsStudent(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new GetDocumentByIdQuery(document.Id),
                CancellationToken.None));
    }
}
