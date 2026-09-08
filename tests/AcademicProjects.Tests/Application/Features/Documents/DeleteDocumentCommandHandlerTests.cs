using AcademicProjects.Application.Features.Documents.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class DeleteDocumentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingDocument_RemovesItAndReturnsTrue()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var document = new Document { FileName = "file.pdf", FilePath = "/file.pdf", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteDocumentCommandHandler(context);

        var result = await handler.Handle(
            new DeleteDocumentCommand(document.Id),
            CancellationToken.None);

        Assert.True(result);
        Assert.Empty(context.Documents);
    }

    [Fact]
    public async Task Handle_NonExistentDocument_ReturnsFalse()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteDocumentCommandHandler(context);

        var result = await handler.Handle(
            new DeleteDocumentCommand(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result);
    }
}
