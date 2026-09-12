using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Documents.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class DeleteDocumentCommandHandlerTests
{
    private static Document SampleDocument(Guid uploaderId, Guid projectId, Project project) => new()
    {
        FileName = "file.pdf",
        StoredFileName = "stored-file.pdf",
        ContentType = "application/pdf",
        FileSizeBytes = 100,
        UploadedById = uploaderId,
        ProjectId = projectId,
        Project = project
    };

    [Fact]
    public async Task Handle_Uploader_RemovesIt()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var uploader = TestCurrentUserService.AsStudent();
        var document = SampleDocument(uploader.UserId!.Value, project.Id, project);
        var uploaderAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = uploader.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        context.ProjectAssignments.Add(uploaderAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteDocumentCommandHandler(
            context, uploader, new ProjectAccessService(context), new ProjectNotificationService(context), new FakeFileStorageService());

        await handler.Handle(
            new DeleteDocumentCommand(document.Id),
            CancellationToken.None);

        Assert.Empty(context.Documents);
    }

    [Fact]
    public async Task Handle_NonExistentDocument_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteDocumentCommandHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context), new ProjectNotificationService(context), new FakeFileStorageService());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new DeleteDocumentCommand(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_OtherProjectMember_RemovesIt()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var uploader = TestCurrentUserService.AsStudent();
        var document = SampleDocument(uploader.UserId!.Value, project.Id, project);
        var teammate = TestCurrentUserService.AsStudent();
        var teammateAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = teammate.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        context.ProjectAssignments.Add(teammateAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteDocumentCommandHandler(
            context, teammate, new ProjectAccessService(context), new ProjectNotificationService(context), new FakeFileStorageService());

        await handler.Handle(
            new DeleteDocumentCommand(document.Id),
            CancellationToken.None);

        Assert.Empty(context.Documents);
    }

    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var uploader = TestCurrentUserService.AsStudent();
        var document = SampleDocument(uploader.UserId!.Value, project.Id, project);
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        await context.SaveChangesAsync(CancellationToken.None);

        var outsider = TestCurrentUserService.AsStudent();
        var handler = new DeleteDocumentCommandHandler(
            context, outsider, new ProjectAccessService(context), new ProjectNotificationService(context), new FakeFileStorageService());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new DeleteDocumentCommand(document.Id),
                CancellationToken.None));
    }
}
