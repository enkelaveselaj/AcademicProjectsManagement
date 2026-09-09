using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Projects.Commands;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.Projects;

public class DeleteProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_ProjectCreator_RemovesIt()
    {
        // A student who started a project can also delete/cancel it themselves.
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var creator = TestCurrentUserService.AsStudent();
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category, CreatedById = creator.UserId!.Value };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteProjectCommandHandler(context, creator);

        await handler.Handle(
            new DeleteProjectCommand(project.Id),
            CancellationToken.None);

        Assert.Empty(context.Projects);
    }

    [Fact]
    public async Task Handle_Administrator_RemovesIt()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteProjectCommandHandler(context, TestCurrentUserService.AsAdministrator());

        await handler.Handle(
            new DeleteProjectCommand(project.Id),
            CancellationToken.None);

        Assert.Empty(context.Projects);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteProjectCommandHandler(context, TestCurrentUserService.AsAdministrator());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new DeleteProjectCommand(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonCreatorMentor_ThrowsForbiddenAccessException()
    {
        // Being the project's mentor isn't enough - only the creator or an administrator can delete it.
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Title", Description = "Description", Status = ProjectStatus.Draft, CategoryId = category.Id, Category = category, CreatedById = Guid.NewGuid() };
        context.Categories.Add(category);
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteProjectCommandHandler(context, TestCurrentUserService.AsMentor());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new DeleteProjectCommand(project.Id),
                CancellationToken.None));
    }
}
