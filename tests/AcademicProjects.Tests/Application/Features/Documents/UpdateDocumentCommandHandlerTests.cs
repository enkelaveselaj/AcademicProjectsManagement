using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Documents.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class UpdateDocumentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingDocument_UpdatesAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var document = new Document { FileName = "old.pdf", FilePath = "/old.pdf", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateDocumentCommandHandler(context);

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
        var handler = new UpdateDocumentCommandHandler(context);

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
        var document = new Document { FileName = "old.pdf", FilePath = "/old.pdf", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateDocumentCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateDocumentCommand(document.Id, "file.pdf", "/file.pdf", Guid.NewGuid()),
                CancellationToken.None));
    }
}
