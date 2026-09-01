using AcademicProjects.Domain.Entities;
using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Tests.Domain;

public class ProjectTests
{
    [Fact]
    public void NewProject_HasDraftStatusAndEmptyCollections()
    {
        var project = new Project();

        Assert.Equal(ProjectStatus.Draft, project.Status);
        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.Empty(project.Milestones);
        Assert.Empty(project.Documents);
        Assert.Empty(project.Comments);
    }
}
