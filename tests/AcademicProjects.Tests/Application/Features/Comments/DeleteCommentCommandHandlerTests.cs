using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Comments.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Comments;

public class DeleteCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingComment_RemovesIt()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var comment = new Comment { Content = "Content", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Comments.Add(comment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteCommentCommandHandler(context);

        await handler.Handle(
            new DeleteCommentCommand(comment.Id),
            CancellationToken.None);

        Assert.Empty(context.Comments);
    }

    [Fact]
    public async Task Handle_NonExistentComment_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteCommentCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new DeleteCommentCommand(Guid.NewGuid()),
                CancellationToken.None));
    }
}
