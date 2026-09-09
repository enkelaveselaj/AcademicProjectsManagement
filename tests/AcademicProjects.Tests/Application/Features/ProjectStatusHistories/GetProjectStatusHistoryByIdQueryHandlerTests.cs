using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectStatusHistories.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectStatusHistories;

public class GetProjectStatusHistoryByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ProjectMember_ReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Submitted, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var history = new ProjectStatusHistory
        {
            ProjectId = project.Id,
            Project = project,
            PreviousStatus = ProjectStatus.Draft,
            NewStatus = ProjectStatus.Submitted,
            Comment = "Ready for review"
        };
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectStatusHistories.Add(history);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectStatusHistoryByIdQueryHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(
            new GetProjectStatusHistoryByIdQuery(history.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(history.Id, result!.Id);
        Assert.Equal("Ready for review", result.Comment);
    }

    [Fact]
    public async Task Handle_NonExistentHistory_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectStatusHistoryByIdQueryHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new GetProjectStatusHistoryByIdQuery(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Submitted, CategoryId = category.Id, Category = category };
        var history = new ProjectStatusHistory
        {
            ProjectId = project.Id,
            Project = project,
            PreviousStatus = ProjectStatus.Draft,
            NewStatus = ProjectStatus.Submitted
        };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectStatusHistories.Add(history);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectStatusHistoryByIdQueryHandler(
            context,
            TestCurrentUserService.AsStudent(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new GetProjectStatusHistoryByIdQuery(history.Id),
                CancellationToken.None));
    }
}
