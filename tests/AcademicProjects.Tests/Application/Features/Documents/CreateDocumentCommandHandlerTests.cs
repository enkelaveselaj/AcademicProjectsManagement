using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Documents.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class CreateDocumentCommandHandlerTests
{
    private static MemoryStream SampleContent() => new([1, 2, 3, 4]);

    [Fact]
    public async Task Handle_ProjectMember_CreatesDocumentAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateDocumentCommandHandler(
            context, student, new ProjectAccessService(context), new ProjectNotificationService(context), new FakeFileStorageService());

        var result = await handler.Handle(
            new CreateDocumentCommand(" report.pdf ", "application/pdf", 4, SampleContent(), project.Id),
            CancellationToken.None);

        Assert.Equal("report.pdf", result.FileName);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal(4, result.FileSizeBytes);
        Assert.Equal(student.UserId, result.UploadedById);
        Assert.Single(context.Documents);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateDocumentCommandHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context), new ProjectNotificationService(context), new FakeFileStorageService());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateDocumentCommand("file.pdf", "application/pdf", 4, SampleContent(), Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateDocumentCommandHandler(
            context,
            TestCurrentUserService.AsStudent(),
            new ProjectAccessService(context), new ProjectNotificationService(context), new FakeFileStorageService());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new CreateDocumentCommand("file.pdf", "application/pdf", 4, SampleContent(), project.Id),
                CancellationToken.None));
    }
}
