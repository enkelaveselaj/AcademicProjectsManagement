using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Comments.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Comments;

public class DeleteCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_Author_RemovesIt()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var author = TestCurrentUserService.AsStudent();
        var comment = new Comment { Content = "Content", AuthorId = author.UserId!.Value, ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Comments.Add(comment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteCommentCommandHandler(context, author);

        await handler.Handle(
            new DeleteCommentCommand(comment.Id),
            CancellationToken.None);

        Assert.Empty(context.Comments);
    }

    [Fact]
    public async Task Handle_NonExistentComment_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteCommentCommandHandler(context, TestCurrentUserService.AsAdministrator());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new DeleteCommentCommand(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DifferentStudent_ThrowsForbiddenAccessException()
    {
        // A student must not be able to delete another student's comment.
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var author = TestCurrentUserService.AsStudent();
        var comment = new Comment { Content = "Content", AuthorId = author.UserId!.Value, ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Comments.Add(comment);
        await context.SaveChangesAsync(CancellationToken.None);

        var otherStudent = TestCurrentUserService.AsStudent();
        var handler = new DeleteCommentCommandHandler(context, otherStudent);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new DeleteCommentCommand(comment.Id),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MentorOfProject_ThrowsForbiddenAccessException()
    {
        // Confirmed rule: even the project's mentor cannot delete another user's comment - only the author or an administrator.
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var author = TestCurrentUserService.AsStudent();
        var comment = new Comment { Content = "Content", AuthorId = author.UserId!.Value, ProjectId = project.Id, Project = project };
        var mentor = TestCurrentUserService.AsMentor();
        var mentorAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = mentor.UserId!.Value, Role = "Mentor" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Comments.Add(comment);
        context.ProjectAssignments.Add(mentorAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteCommentCommandHandler(context, mentor);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new DeleteCommentCommand(comment.Id),
                CancellationToken.None));
    }
}
