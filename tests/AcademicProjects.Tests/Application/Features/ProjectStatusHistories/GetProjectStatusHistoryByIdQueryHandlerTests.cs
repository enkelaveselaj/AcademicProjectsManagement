using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.ProjectStatusHistories.Queries;
using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;
using AcademicProjects.Tests.TestHelpers;

namespace AcademicProjects.Tests.Application.Features.ProjectStatusHistories;

public class GetProjectStatusHistoryByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ExistingHistory_ReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var category = new Category { Name = "Category" };
        var project = new Project { Title = "Project", Description = "Desc", Status = ProjectStatus.Submitted, CategoryId = category.Id, Category = category };
        var history = new ProjectStatusHistory
        {
            ProjectId = project.Id,
            Project = project,
            PreviousStatus = ProjectStatus.Draft,
            NewStatus = ProjectStatus.Submitted,
            Comment = "Ready for review"
        };
        context.Categories.Add(category);
        context.Projects.Add(project);
        context.ProjectStatusHistories.Add(history);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetProjectStatusHistoryByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetProjectStatusHistoryByIdQuery(history.Id),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(history.Id, result!.Id);
        Assert.Equal("Ready for review", result.Comment);
    }

    [Fact]
    public async Task Handle_NonExistentHistory_ThrowsNotFoundException()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetProjectStatusHistoryByIdQueryHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new GetProjectStatusHistoryByIdQuery(Guid.NewGuid()),
                CancellationToken.None));
    }
}
