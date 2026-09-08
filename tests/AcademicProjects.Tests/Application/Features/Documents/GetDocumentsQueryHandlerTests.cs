using AcademicProjects.Application.Features.Documents.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class GetDocumentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllDocuments()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Documents.AddRange(
            new Document { FileName = "a.pdf", FilePath = "/a.pdf", ProjectId = project.Id, Project = project },
            new Document { FileName = "b.pdf", FilePath = "/b.pdf", ProjectId = project.Id, Project = project });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetDocumentsQueryHandler(context);

        var result = await handler.Handle(new GetDocumentsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_NoDocuments_ReturnsEmptyList()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetDocumentsQueryHandler(context);

        var result = await handler.Handle(new GetDocumentsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
