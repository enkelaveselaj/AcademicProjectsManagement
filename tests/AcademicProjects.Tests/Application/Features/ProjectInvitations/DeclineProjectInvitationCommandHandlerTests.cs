using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Features.ProjectInvitations.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectInvitations;

public class DeclineProjectInvitationCommandHandlerTests
{
    [Fact]
    public async Task Handle_PendingInvitation_MarksDeclinedAndNotifiesInviter()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var invitedStudent = TestCurrentUserService.AsStudent();
        var inviterId = Guid.NewGuid();
        var invitation = new ProjectInvitation { ProjectId = project.Id, Project = project, InvitedUserId = invitedStudent.UserId!.Value, InvitedById = inviterId, Role = "Student", Status = InvitationStatus.Pending };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectInvitations.Add(invitation);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeclineProjectInvitationCommandHandler(context, invitedStudent, new ProjectNotificationService(context));

        var result = await handler.Handle(new DeclineProjectInvitationCommand(invitation.Id), CancellationToken.None);

        Assert.Equal(InvitationStatus.Declined, result.Status);
        Assert.Contains(context.Notifications, n => n.UserId == inviterId);
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

        var handler = new DeclineProjectInvitationCommandHandler(context, TestCurrentUserService.AsStudent(), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new DeclineProjectInvitationCommand(invitation.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentInvitation_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeclineProjectInvitationCommandHandler(context, TestCurrentUserService.AsAdministrator(), new ProjectNotificationService(context));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new DeclineProjectInvitationCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
