using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectAssignments.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectAssignments;

public class GetProjectAssignmentByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_OwnAssignment_ReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectAssignmentByIdQueryHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(
            new GetProjectAssignmentByIdQuery(assignment.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(assignment.Id, result!.Id);
    }

    [Fact]
    public async Task Handle_NonExistentAssignment_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectAssignmentByIdQueryHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new GetProjectAssignmentByIdQuery(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnrelatedStudent_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = Guid.NewGuid(), Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectAssignmentByIdQueryHandler(
            context,
            TestCurrentUserService.AsStudent(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new GetProjectAssignmentByIdQuery(assignment.Id),
                CancellationToken.None));
    }
}
