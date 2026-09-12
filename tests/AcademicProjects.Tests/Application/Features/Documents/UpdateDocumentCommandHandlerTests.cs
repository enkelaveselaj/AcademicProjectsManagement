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
    private static Document SampleDocument(Guid uploaderId, Guid projectId, Project project) => new()
    {
        FileName = "old.pdf",
        StoredFileName = "stored-old.pdf",
        ContentType = "application/pdf",
        FileSizeBytes = 100,
        UploadedById = uploaderId,
        ProjectId = projectId,
        Project = project
    };

    [Fact]
    public async Task Handle_Uploader_UpdatesAndReturnsDto()
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

        var handler = new UpdateDocumentCommandHandler(context, uploader, new ProjectAccessService(context), new ProjectNotificationService(context));

        var result = await handler.Handle(
            new UpdateDocumentCommand(document.Id, " new.pdf ", project.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("new.pdf", result!.FileName);
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
                new UpdateDocumentCommand(Guid.NewGuid(), "file.pdf", Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var admin = TestCurrentUserService.AsAdministrator();
        var document = SampleDocument(Guid.NewGuid(), project.Id, project);
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateDocumentCommandHandler(context, admin, new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateDocumentCommand(document.Id, "file.pdf", Guid.NewGuid()),
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
        var document = SampleDocument(uploader.UserId!.Value, project.Id, project);
        var teammate = TestCurrentUserService.AsStudent();
        var teammateAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = teammate.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        context.ProjectAssignments.Add(teammateAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateDocumentCommandHandler(context, teammate, new ProjectAccessService(context), new ProjectNotificationService(context));

        var result = await handler.Handle(
            new UpdateDocumentCommand(document.Id, "renamed.pdf", project.Id),
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
        var document = SampleDocument(uploader.UserId!.Value, project.Id, project);
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        await context.SaveChangesAsync(CancellationToken.None);

        var outsider = TestCurrentUserService.AsStudent();
        var handler = new UpdateDocumentCommandHandler(context, outsider, new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new UpdateDocumentCommand(document.Id, "renamed.pdf", project.Id),
                CancellationToken.None));
    }
}
