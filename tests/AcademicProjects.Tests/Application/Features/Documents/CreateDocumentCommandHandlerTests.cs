using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Documents.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class CreateDocumentCommandHandlerTests
{
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

        var handler = new CreateDocumentCommandHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(
            new CreateDocumentCommand(" report.pdf ", " /files/report.pdf ", project.Id),
            CancellationToken.None);

        Assert.Equal("report.pdf", result.FileName);
        Assert.Equal("/files/report.pdf", result.FilePath);
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
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateDocumentCommand("file.pdf", "/files/file.pdf", Guid.NewGuid()),
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
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new CreateDocumentCommand("file.pdf", "/files/file.pdf", project.Id),
                CancellationToken.None));
    }
}
