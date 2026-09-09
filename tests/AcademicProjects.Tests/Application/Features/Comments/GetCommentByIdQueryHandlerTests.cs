using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Comments.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Comments;

public class GetCommentByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ProjectMember_ReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var comment = new Comment { Content = "Content", ProjectId = project.Id, Project = project };
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Comments.Add(comment);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetCommentByIdQueryHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(
            new GetCommentByIdQuery(comment.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(comment.Id, result!.Id);
    }

    [Fact]
    public async Task Handle_NonExistentComment_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetCommentByIdQueryHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new GetCommentByIdQuery(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var comment = new Comment { Content = "Content", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.Comments.Add(comment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetCommentByIdQueryHandler(
            context,
            TestCurrentUserService.AsStudent(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new GetCommentByIdQuery(comment.Id),
                CancellationToken.None));
    }
}
