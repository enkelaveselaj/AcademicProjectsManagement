using AcademicProjects.Application.Features.Documents.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class CreateDocumentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProject_CreatesDocumentAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateDocumentCommandHandler(context);

        var result = await handler.Handle(
            new CreateDocumentCommand(" report.pdf ", " /files/report.pdf ", project.Id),
            CancellationToken.None);

        Assert.Equal("report.pdf", result.FileName);
        Assert.Equal("/files/report.pdf", result.FilePath);
        Assert.Single(context.Documents);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsKeyNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateDocumentCommandHandler(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(
                new CreateDocumentCommand("file.pdf", "/files/file.pdf", Guid.NewGuid()),
                CancellationToken.None));
    }
}
