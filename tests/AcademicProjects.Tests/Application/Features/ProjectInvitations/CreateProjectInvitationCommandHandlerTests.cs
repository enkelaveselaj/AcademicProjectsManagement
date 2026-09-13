using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.ProjectInvitations.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectInvitations;

public class CreateProjectInvitationCommandHandlerTests
{
    [Fact]
    public async Task Handle_StudentInvitesAnotherStudent_CreatesPendingInvitation()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var studentAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(studentAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var invitedUserId = Guid.NewGuid();
        var userManagement = new FakeUserManagementService().WithUser(invitedUserId, "Student");

        var handler = new CreateProjectInvitationCommandHandler(
            context, student, new ProjectAccessService(context), userManagement, new ProjectNotificationService(context));

        var result = await handler.Handle(
            new CreateProjectInvitationCommand(project.Id, invitedUserId, "Student"),
            CancellationToken.None);

        Assert.Equal(InvitationStatus.Pending, result.Status);
        Assert.Equal(invitedUserId, result.InvitedUserId);
        Assert.Equal(student.UserId, result.InvitedById);
        Assert.Contains(context.Notifications, n => n.UserId == invitedUserId);
    }

    [Fact]
    public async Task Handle_InvitedUserDoesNotHoldTheRequestedRole_ThrowsConflictException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var studentAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(studentAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var invitedUserId = Guid.NewGuid();
        var userManagement = new FakeUserManagementService().WithUser(invitedUserId, "Student");
        var handler = new CreateProjectInvitationCommandHandler(
            context, student, new ProjectAccessService(context), userManagement, new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new CreateProjectInvitationCommand(project.Id, invitedUserId, "Mentor"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvitedUserAlreadyAMember_ThrowsConflictException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var existingMemberId = Guid.NewGuid();
        var studentAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        var existingMemberAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = existingMemberId, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.AddRange(studentAssignment, existingMemberAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var userManagement = new FakeUserManagementService().WithUser(existingMemberId, "Student");
        var handler = new CreateProjectInvitationCommandHandler(
            context, student, new ProjectAccessService(context), userManagement, new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new CreateProjectInvitationCommand(project.Id, existingMemberId, "Student"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DuplicatePendingInvitation_ThrowsConflictException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var invitedUserId = Guid.NewGuid();
        var studentAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        var existingInvitation = new ProjectInvitation { ProjectId = project.Id, Project = project, InvitedUserId = invitedUserId, InvitedById = student.UserId.Value, Role = "Student", Status = InvitationStatus.Pending };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(studentAssignment);
        context.ProjectInvitations.Add(existingInvitation);
        await context.SaveChangesAsync(CancellationToken.None);

        var userManagement = new FakeUserManagementService().WithUser(invitedUserId, "Student");
        var handler = new CreateProjectInvitationCommandHandler(
            context, student, new ProjectAccessService(context), userManagement, new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new CreateProjectInvitationCommand(project.Id, invitedUserId, "Student"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ProjectAlreadyHasMentor_ThrowsConflictException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var studentAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        var mentorAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = Guid.NewGuid(), Role = "Mentor" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.AddRange(studentAssignment, mentorAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var newMentorId = Guid.NewGuid();
        var userManagement = new FakeUserManagementService().WithUser(newMentorId, "Mentor");
        var handler = new CreateProjectInvitationCommandHandler(
            context, student, new ProjectAccessService(context), userManagement, new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new CreateProjectInvitationCommand(project.Id, newMentorId, "Mentor"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MentorRequestAlreadyPending_ThrowsConflictException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var studentAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        var pendingMentorInvite = new ProjectInvitation { ProjectId = project.Id, Project = project, InvitedUserId = Guid.NewGuid(), InvitedById = student.UserId.Value, Role = "Mentor", Status = InvitationStatus.Pending };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(studentAssignment);
        context.ProjectInvitations.Add(pendingMentorInvite);
        await context.SaveChangesAsync(CancellationToken.None);

        var newMentorId = Guid.NewGuid();
        var userManagement = new FakeUserManagementService().WithUser(newMentorId, "Mentor");
        var handler = new CreateProjectInvitationCommandHandler(
            context, student, new ProjectAccessService(context), userManagement, new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new CreateProjectInvitationCommand(project.Id, newMentorId, "Mentor"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvitingSelf_ThrowsConflictException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var studentAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(studentAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var userManagement = new FakeUserManagementService().WithUser(student.UserId!.Value, "Student");
        var handler = new CreateProjectInvitationCommandHandler(
            context, student, new ProjectAccessService(context), userManagement, new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new CreateProjectInvitationCommand(project.Id, student.UserId.Value, "Student"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var userManagement = new FakeUserManagementService().WithUser(Guid.NewGuid(), "Student");
        var handler = new CreateProjectInvitationCommandHandler(
            context, TestCurrentUserService.AsAdministrator(), new ProjectAccessService(context), userManagement, new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new CreateProjectInvitationCommand(Guid.NewGuid(), Guid.NewGuid(), "Student"),
                CancellationToken.None));
    }
}
