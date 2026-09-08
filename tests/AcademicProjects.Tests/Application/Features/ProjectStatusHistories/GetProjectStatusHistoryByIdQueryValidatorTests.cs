using AcademicProjects.Application.Features.ProjectStatusHistories.Queries;

namespace AcademicProjects.Tests.Application.Features.ProjectStatusHistories;

public class GetProjectStatusHistoryByIdQueryValidatorTests
{
    private readonly GetProjectStatusHistoryByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_NonEmptyId_IsValid()
    {
        var result = _validator.Validate(new GetProjectStatusHistoryByIdQuery(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(new GetProjectStatusHistoryByIdQuery(Guid.Empty));

        Assert.False(result.IsValid);
    }
}
