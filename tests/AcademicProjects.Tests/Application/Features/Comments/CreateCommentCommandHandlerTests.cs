using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Comments.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Comments;

public class CreateCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProject_CreatesCommentAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateCommentCommandHandler(context);

        var result = await handler.Handle(
            new CreateCommentCommand(" Looks good ", project.Id),
            CancellationToken.None);

        Assert.Equal("Looks good", result.Content);
        Assert.Equal(project.Id, result.ProjectId);
        Assert.Single(context.Comments);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateCommentCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateCommentCommand("Content", Guid.NewGuid()),
                CancellationToken.None));
    }
}
