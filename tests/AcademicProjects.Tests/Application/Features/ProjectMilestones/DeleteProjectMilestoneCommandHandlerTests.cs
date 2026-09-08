using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectMilestones.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class DeleteProjectMilestoneCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingMilestone_RemovesIt()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        var milestone = new ProjectMilestone { Title = "Milestone", ProjectId = project.Id, Project = project };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectMilestones.Add(milestone);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteProjectMilestoneCommandHandler(context);

        await handler.Handle(
            new DeleteProjectMilestoneCommand(milestone.Id),
            CancellationToken.None);

        Assert.Empty(context.ProjectMilestones);
    }

    [Fact]
    public async Task Handle_NonExistentMilestone_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteProjectMilestoneCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new DeleteProjectMilestoneCommand(Guid.NewGuid()),
                CancellationToken.None));
    }
}
