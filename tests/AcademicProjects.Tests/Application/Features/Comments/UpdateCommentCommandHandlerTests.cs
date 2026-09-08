using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Comments.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Comments;

public class UpdateCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingComment_UpdatesAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var comment = new Comment { Content = "Old", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Comments.Add(comment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateCommentCommandHandler(context);

        var result = await handler.Handle(
            new UpdateCommentCommand(comment.Id, " New content ", project.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New content", result!.Content);
    }

    [Fact]
    public async Task Handle_NonExistentComment_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdateCommentCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateCommentCommand(Guid.NewGuid(), "Content", Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var comment = new Comment { Content = "Old", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Comments.Add(comment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateCommentCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateCommentCommand(comment.Id, "Content", Guid.NewGuid()),
                CancellationToken.None));
    }
}
