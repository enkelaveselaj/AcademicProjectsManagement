using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectAssignments.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectAssignments;

public class DeleteProjectAssignmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ProjectMentor_RemovesIt()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var mentor = TestCurrentUserService.AsMentor();
        var mentorAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = mentor.UserId!.Value, Role = "Mentor" };
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = Guid.NewGuid(), Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.AddRange(mentorAssignment, assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteProjectAssignmentCommandHandler(context, mentor, new ProjectAccessService(context));

        await handler.Handle(
            new DeleteProjectAssignmentCommand(assignment.Id),
            CancellationToken.None);

        Assert.Single(context.ProjectAssignments);
    }

    [Fact]
    public async Task Handle_NonExistentAssignment_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteProjectAssignmentCommandHandler(
            context,
            TestCurrentUserService.AsAdministrator(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new DeleteProjectAssignmentCommand(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnrelatedMentor_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = Guid.NewGuid(), Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteProjectAssignmentCommandHandler(
            context,
            TestCurrentUserService.AsMentor(),
            new ProjectAccessService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new DeleteProjectAssignmentCommand(assignment.Id),
                CancellationToken.None));
    }
}
