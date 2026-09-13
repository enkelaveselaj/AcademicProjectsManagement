using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Projects.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Projects;

public class GetProjectByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ProjectMember_ReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectByIdQueryHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(
            new GetProjectByIdQuery(project.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(project.Id, result!.Id);
        Assert.Equal(category.Name, result.CategoryName);
    }

    [Fact]
    public async Task Handle_Administrator_ReturnsDtoEvenWithoutMembership()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectByIdQueryHandler(context, TestCurrentUserService.AsAdministrator(), new ProjectAccessService(context));

        var result = await handler.Handle(
            new GetProjectByIdQuery(project.Id),
            CancellationToken.None);

        Assert.Equal(project.Id, result.Id);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectByIdQueryHandler(
            context, TestCurrentUserService.AsAdministrator(), new ProjectAccessService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new GetProjectByIdQuery(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectByIdQueryHandler(
            context, TestCurrentUserService.AsStudent(), new ProjectAccessService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new GetProjectByIdQuery(project.Id),
                CancellationToken.None));
    }
}
