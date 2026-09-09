using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Documents.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class UpdateDocumentCommandHandlerTests
{
    [Fact]
    public async Task Handle_Uploader_UpdatesAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var uploader = TestCurrentUserService.AsStudent();
        var document = new Document { FileName = "old.pdf", FilePath = "/old.pdf", UploadedById = uploader.UserId!.Value, ProjectId = project.Id, Project = project };
        var uploaderAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = uploader.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        context.ProjectAssignments.Add(uploaderAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateDocumentCommandHandler(context, uploader, new ProjectAccessService(context), new ProjectNotificationService(context));

        var result = await handler.Handle(
            new UpdateDocumentCommand(document.Id, " new.pdf ", " /new.pdf ", project.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("new.pdf", result!.FileName);
        Assert.Equal("/new.pdf", result.FilePath);
    }

    [Fact]
    public async Task Handle_NonExistentDocument_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdateDocumentCommandHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateDocumentCommand(Guid.NewGuid(), "file.pdf", "/file.pdf", Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var admin = TestCurrentUserService.AsAdministrator();
        var document = new Document { FileName = "old.pdf", FilePath = "/old.pdf", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateDocumentCommandHandler(context, admin, new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateDocumentCommand(document.Id, "file.pdf", "/file.pdf", Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_OtherProjectMember_UpdatesAndReturnsDto()
    {
        // Documents are a shared workspace - any teammate can update them, not just the uploader.
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var uploader = TestCurrentUserService.AsStudent();
        var document = new Document { FileName = "old.pdf", FilePath = "/old.pdf", UploadedById = uploader.UserId!.Value, ProjectId = project.Id, Project = project };
        var teammate = TestCurrentUserService.AsStudent();
        var teammateAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = teammate.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        context.ProjectAssignments.Add(teammateAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateDocumentCommandHandler(context, teammate, new ProjectAccessService(context), new ProjectNotificationService(context));

        var result = await handler.Handle(
            new UpdateDocumentCommand(document.Id, "renamed.pdf", "/renamed.pdf", project.Id),
            CancellationToken.None);

        Assert.Equal("renamed.pdf", result.FileName);
    }

    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var uploader = TestCurrentUserService.AsStudent();
        var document = new Document { FileName = "old.pdf", FilePath = "/old.pdf", UploadedById = uploader.UserId!.Value, ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        await context.SaveChangesAsync(CancellationToken.None);

        var outsider = TestCurrentUserService.AsStudent();
        var handler = new UpdateDocumentCommandHandler(context, outsider, new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new UpdateDocumentCommand(document.Id, "renamed.pdf", "/renamed.pdf", project.Id),
                CancellationToken.None));
    }
}
