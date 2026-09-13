using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Behaviors;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Common.Behaviors;

public sealed record FakeAdminOnlyRequest : IRequireAdministrator;

public sealed record FakeProjectScopedRequest(Guid ProjectId, ProjectAccessLevel RequiredAccess) : IProjectScopedRequest;

public sealed record FakeUnrestrictedRequest;

public class AuthorizationBehaviorTests
{
    private static Task<string> Next() => Task.FromResult("ok");

    [Fact]
    public async Task Handle_AdminOnlyRequest_AdministratorIsAllowed()
    {
        using var context = TestDbContextFactory.Create();
        var behavior = new AuthorizationBehavior<FakeAdminOnlyRequest, string>(
            TestCurrentUserService.AsAdministrator(), new ProjectAccessService(context));

        var result = await behavior.Handle(new FakeAdminOnlyRequest(), _ => Next(), CancellationToken.None);

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_AdminOnlyRequest_NonAdministratorIsForbidden()
    {
        using var context = TestDbContextFactory.Create();
        var behavior = new AuthorizationBehavior<FakeAdminOnlyRequest, string>(
            TestCurrentUserService.AsMentor(), new ProjectAccessService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            behavior.Handle(new FakeAdminOnlyRequest(), _ => Next(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ProjectScopedRequest_MemberIsAllowed()
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

        var behavior = new AuthorizationBehavior<FakeProjectScopedRequest, string>(student, new ProjectAccessService(context));

        var result = await behavior.Handle(
            new FakeProjectScopedRequest(project.Id, ProjectAccessLevel.Member), _ => Next(), CancellationToken.None);

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_ProjectScopedRequest_NonMemberIsForbidden()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var behavior = new AuthorizationBehavior<FakeProjectScopedRequest, string>(
            TestCurrentUserService.AsStudent(), new ProjectAccessService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            behavior.Handle(
                new FakeProjectScopedRequest(project.Id, ProjectAccessLevel.Member), _ => Next(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ProjectScopedRequest_MentorRequiredButUserIsOnlyAMember_ThrowsForbidden()
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

        var behavior = new AuthorizationBehavior<FakeProjectScopedRequest, string>(student, new ProjectAccessService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            behavior.Handle(
                new FakeProjectScopedRequest(project.Id, ProjectAccessLevel.Mentor), _ => Next(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ProjectScopedRequest_MentorRequiredAndUserIsMentor_Allowed()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var mentor = TestCurrentUserService.AsMentor();
        var assignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = mentor.UserId!.Value, Role = "Mentor" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var behavior = new AuthorizationBehavior<FakeProjectScopedRequest, string>(mentor, new ProjectAccessService(context));

        var result = await behavior.Handle(
            new FakeProjectScopedRequest(project.Id, ProjectAccessLevel.Mentor), _ => Next(), CancellationToken.None);

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_ProjectScopedRequest_AdministratorBypassesMembershipCheck()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var behavior = new AuthorizationBehavior<FakeProjectScopedRequest, string>(
            TestCurrentUserService.AsAdministrator(), new ProjectAccessService(context));

        var result = await behavior.Handle(
            new FakeProjectScopedRequest(project.Id, ProjectAccessLevel.Mentor), _ => Next(), CancellationToken.None);

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_UnrestrictedRequest_AlwaysAllowed()
    {
        using var context = TestDbContextFactory.Create();
        var behavior = new AuthorizationBehavior<FakeUnrestrictedRequest, string>(
            TestCurrentUserService.AsStudent(), new ProjectAccessService(context));

        var result = await behavior.Handle(new FakeUnrestrictedRequest(), _ => Next(), CancellationToken.None);

        Assert.Equal("ok", result);
    }
}
