using AcademicProjects.Application.Features.ProjectAssignments.Queries;

namespace AcademicProjects.Tests.Application.Features.ProjectAssignments;

public class GetProjectAssignmentByIdQueryValidatorTests
{
    private readonly GetProjectAssignmentByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_NonEmptyId_IsValid()
    {
        var result = _validator.Validate(new GetProjectAssignmentByIdQuery(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(new GetProjectAssignmentByIdQuery(Guid.Empty));

        Assert.False(result.IsValid);
    }
}
