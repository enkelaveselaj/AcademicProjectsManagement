using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.ProjectInvitations.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectInvitations;

public class AcceptProjectInvitationCommandHandlerTests
{
    [Fact]
    public async Task Handle_PendingInvitation_CreatesAssignmentAndMarksAccepted()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var invitedStudent = TestCurrentUserService.AsStudent();
        var invitation = new ProjectInvitation { ProjectId = project.Id, Project = project, InvitedUserId = invitedStudent.UserId!.Value, InvitedById = Guid.NewGuid(), Role = "Student", Status = InvitationStatus.Pending };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectInvitations.Add(invitation);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new AcceptProjectInvitationCommandHandler(
            context, invitedStudent, new ProjectAccessService(context), new ProjectNotificationService(context));

        var result = await handler.Handle(new AcceptProjectInvitationCommand(invitation.Id), CancellationToken.None);

        Assert.Equal(InvitationStatus.Accepted, result.Status);
        Assert.Contains(context.ProjectAssignments, a => a.ProjectId == project.Id && a.UserId == invitedStudent.UserId && a.Role == "Student");
    }

    [Fact]
    public async Task Handle_NotTheInvitedUser_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var invitation = new ProjectInvitation { ProjectId = project.Id, Project = project, InvitedUserId = Guid.NewGuid(), InvitedById = Guid.NewGuid(), Role = "Student", Status = InvitationStatus.Pending };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectInvitations.Add(invitation);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new AcceptProjectInvitationCommandHandler(
            context, TestCurrentUserService.AsStudent(), new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new AcceptProjectInvitationCommand(invitation.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyAccepted_ThrowsConflictException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var invitedStudent = TestCurrentUserService.AsStudent();
        var invitation = new ProjectInvitation { ProjectId = project.Id, Project = project, InvitedUserId = invitedStudent.UserId!.Value, InvitedById = Guid.NewGuid(), Role = "Student", Status = InvitationStatus.Accepted };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectInvitations.Add(invitation);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new AcceptProjectInvitationCommandHandler(
            context, invitedStudent, new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new AcceptProjectInvitationCommand(invitation.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MentorRequestButProjectAlreadyHasMentor_ThrowsConflictException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var invitedMentor = TestCurrentUserService.AsMentor();
        var invitation = new ProjectInvitation { ProjectId = project.Id, Project = project, InvitedUserId = invitedMentor.UserId!.Value, InvitedById = Guid.NewGuid(), Role = "Mentor", Status = InvitationStatus.Pending };
        var existingMentorAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = Guid.NewGuid(), Role = "Mentor" };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectInvitations.Add(invitation);
        context.ProjectAssignments.Add(existingMentorAssignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new AcceptProjectInvitationCommandHandler(
            context, invitedMentor, new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new AcceptProjectInvitationCommand(invitation.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentInvitation_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new AcceptProjectInvitationCommandHandler(
            context, TestCurrentUserService.AsAdministrator(), new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new AcceptProjectInvitationCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
