using AcademicProjects.Application.Features.Documents.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class GetDocumentByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ExistingDocument_ReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var document = new Document { FileName = "file.pdf", FilePath = "/file.pdf", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.Add(document);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetDocumentByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetDocumentByIdQuery(document.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(document.Id, result!.Id);
    }

    [Fact]
    public async Task Handle_NonExistentDocument_ReturnsNull()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetDocumentByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetDocumentByIdQuery(Guid.NewGuid()),
            CancellationToken.None);

        Assert.Null(result);
    }
}
