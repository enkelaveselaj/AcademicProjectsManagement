using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.ProjectInvitations.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectInvitations;

public class CancelProjectInvitationCommandHandlerTests
{
    [Fact]
    public async Task Handle_PendingInvitation_RemovesItAndNotifiesInvitedUser()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();
        var invitedUserId = Guid.NewGuid();
        var studentAssignment = new ProjectAssignment { ProjectId = project.Id, Project = project, UserId = student.UserId!.Value, Role = "Student" };
        var invitation = new ProjectInvitation { ProjectId = project.Id, Project = project, InvitedUserId = invitedUserId, InvitedById = student.UserId.Value, Role = "Student", Status = InvitationStatus.Pending };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectAssignments.Add(studentAssignment);
        context.ProjectInvitations.Add(invitation);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CancelProjectInvitationCommandHandler(context, student, new ProjectAccessService(context), new ProjectNotificationService(context));

        await handler.Handle(new CancelProjectInvitationCommand(invitation.Id), CancellationToken.None);

        Assert.Empty(context.ProjectInvitations);
        Assert.Contains(context.Notifications, n => n.UserId == invitedUserId);
    }

    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var invitation = new ProjectInvitation { ProjectId = project.Id, Project = project, InvitedUserId = Guid.NewGuid(), InvitedById = Guid.NewGuid(), Role = "Student", Status = InvitationStatus.Pending };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectInvitations.Add(invitation);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CancelProjectInvitationCommandHandler(context, TestCurrentUserService.AsStudent(), new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new CancelProjectInvitationCommand(invitation.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyAccepted_ThrowsConflictException()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var invitation = new ProjectInvitation { ProjectId = project.Id, Project = project, InvitedUserId = Guid.NewGuid(), InvitedById = Guid.NewGuid(), Role = "Student", Status = InvitationStatus.Accepted };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectInvitations.Add(invitation);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CancelProjectInvitationCommandHandler(context, TestCurrentUserService.AsAdministrator(), new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CancelProjectInvitationCommand(invitation.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentInvitation_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CancelProjectInvitationCommandHandler(context, TestCurrentUserService.AsAdministrator(), new ProjectAccessService(context), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CancelProjectInvitationCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
