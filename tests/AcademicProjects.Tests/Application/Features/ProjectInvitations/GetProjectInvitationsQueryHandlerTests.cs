using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.ProjectInvitations.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectInvitations;

public class GetProjectInvitationsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Administrator_ReturnsAllInvitations()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var invitationOne = new ProjectInvitation { ProjectId = project.Id, Project = project, InvitedUserId = Guid.NewGuid(), InvitedById = Guid.NewGuid(), Role = "Student", Status = InvitationStatus.Pending };
        var invitationTwo = new ProjectInvitation { ProjectId = project.Id, Project = project, InvitedUserId = Guid.NewGuid(), InvitedById = Guid.NewGuid(), Role = "Mentor", Status = InvitationStatus.Pending };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectInvitations.AddRange(invitationOne, invitationTwo);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectInvitationsQueryHandler(context, TestCurrentUserService.AsAdministrator(), new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectInvitationsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_Student_SeesOwnInvitationAndProjectInvitationsButNotUnrelatedOnes()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var ownProject = new Project { Title = "Own Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var otherProject = new Project { Title = "Other Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var student = TestCurrentUserService.AsStudent();

        var assignment = new ProjectAssignment { ProjectId = ownProject.Id, Project = ownProject, UserId = student.UserId!.Value, Role = "Student" };
        var invitationAddressedToStudent = new ProjectInvitation { ProjectId = otherProject.Id, Project = otherProject, InvitedUserId = student.UserId.Value, InvitedById = Guid.NewGuid(), Role = "Student", Status = InvitationStatus.Pending };
        var invitationOnOwnProject = new ProjectInvitation { ProjectId = ownProject.Id, Project = ownProject, InvitedUserId = Guid.NewGuid(), InvitedById = student.UserId.Value, Role = "Mentor", Status = InvitationStatus.Pending };
        var unrelatedInvitation = new ProjectInvitation { ProjectId = otherProject.Id, Project = otherProject, InvitedUserId = Guid.NewGuid(), InvitedById = Guid.NewGuid(), Role = "Student", Status = InvitationStatus.Pending };

        context.Categories.Add(category);
        context.Projects.AddRange(ownProject, otherProject);
        context.ProjectAssignments.Add(assignment);
        context.ProjectInvitations.AddRange(invitationAddressedToStudent, invitationOnOwnProject, unrelatedInvitation);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectInvitationsQueryHandler(context, student, new ProjectAccessService(context));

        var result = await handler.Handle(new GetProjectInvitationsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, i => i.Id == invitationAddressedToStudent.Id);
        Assert.Contains(result, i => i.Id == invitationOnOwnProject.Id);
        Assert.DoesNotContain(result, i => i.Id == unrelatedInvitation.Id);
    }
}
