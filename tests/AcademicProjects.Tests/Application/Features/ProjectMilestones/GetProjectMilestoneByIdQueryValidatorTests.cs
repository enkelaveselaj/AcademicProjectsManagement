using AcademicProjects.Application.Features.ProjectMilestones.Queries;

namespace AcademicProjects.Tests.Application.Features.ProjectMilestones;

public class GetProjectMilestoneByIdQueryValidatorTests
{
    private readonly GetProjectMilestoneByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_NonEmptyId_IsValid()
    {
        var result = _validator.Validate(new GetProjectMilestoneByIdQuery(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(new GetProjectMilestoneByIdQuery(Guid.Empty));

        Assert.False(result.IsValid);
    }
}
